// <copyright file="CipherPool.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Xobex.Cryptography;

/// <summary>
/// A pool of disposable objects.
/// </summary>
/// <typeparam name="T">The type of disposable objects to pool.</typeparam>
public sealed class DisposableObjectPool<T> : IDisposable
    where T : IDisposable
{
    private readonly ConcurrentStack<T> _pool = [];

    private readonly Func<T> _createObject;
    private readonly Action<T>? _returnObject;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableObjectPool{T}"/> class.
    /// </summary>
    /// <param name="createObject">Factory function</param>
    /// <param name="returnObject">Return action</param>
    public DisposableObjectPool(Func<T> createObject, Action<T>? returnObject = null)
    {
        _createObject = createObject ?? throw new ArgumentNullException(nameof(createObject));
        _returnObject = returnObject;
    }

    /// <summary>
    /// Disposes the pool and all pooled objects
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }
        DrainPool();
    }

    private void DrainPool()
    {
        List<Exception>? exceptions = null;
        while (_pool.TryPop(out var instance))
        {
            try
            {
                instance.Dispose();
            }
            catch(Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }
    }

    /// <summary>
    /// Leases the object from the pool or creates new object if pool is empty
    /// </summary>
    /// <returns>Disposable Wrapper to the pooled object</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ObjectInstance LeaseObject()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) == 1, this);
        if (_pool.TryPop(out var value))
        {
            return new ObjectInstance(this, value);
        }
        return new ObjectInstance(this, _createObject());
    }

    /// <summary>
    /// Returns object to the pool
    /// </summary>
    /// <param name="value"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnObject(T value)
    {
        if (Volatile.Read(ref _disposed) == 1)
        {
            value.Dispose();
            return;
        }
        try
        {
            _returnObject?.Invoke(value);
        }
        catch
        {
            value.Dispose();
            throw;
        }
        _pool.Push(value);
        if (Volatile.Read(ref _disposed) == 1)
        {
            DrainPool();
        }
    }

    /// <summary>
    /// Pooled object instance wrapper
    /// </summary>
    public readonly ref struct ObjectInstance
    {
        private readonly DisposableObjectPool<T> _owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectInstance"/> class.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="instance"></param>
        internal ObjectInstance(DisposableObjectPool<T> owner, T instance)
        {
            _owner = owner;
            Instance = instance;
        }

        /// <summary>
        /// Gets the instance of the pooled object
        /// </summary>
        public T Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Returns the object to the pool
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _owner.ReturnObject(Instance);
        }
    }
}
