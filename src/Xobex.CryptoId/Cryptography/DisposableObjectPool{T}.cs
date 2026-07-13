// <copyright file="DisposableObjectPool{T}.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;

namespace Xobex.Cryptography;

/// <summary>
/// A pool of disposable objects.
/// </summary>
/// <typeparam name="T">The type of disposable objects to pool.</typeparam>
public sealed class DisposableObjectPool<T> : IDisposable
    where T : class, IDisposable
{
    private readonly Lock _sync = new();
    private readonly Stack<T> _pool = [];

    private readonly Func<T> _createObject;
    private readonly Action<T>? _returnObject;
    private readonly Action<Exception>? _logError;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableObjectPool{T}"/> class.
    /// </summary>
    /// <param name="createObject">Factory function</param>
    /// <param name="returnObject">Return action</param>
    /// <param name="logError"></param>
    public DisposableObjectPool(Func<T> createObject, Action<T>? returnObject = null, Action<Exception>? logError = null)
    {
        _createObject = createObject ?? throw new ArgumentNullException(nameof(createObject));
        _returnObject = returnObject;
        _logError = logError;
    }

    /// <summary>
    /// Disposes the pool and all pooled objects
    /// </summary>
    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            while (_pool.TryPop(out var instance))
            {
                SafeDispose(instance);
            }
        }
    }

    private void SafeDispose(T? instance)
    {
        try
        {
            instance?.Dispose();
        }
        catch (Exception ex)
        {
            _logError?.Invoke(ex);
        }
    }

    /// <summary>
    /// Leases the object from the pool or creates new object if pool is empty
    /// </summary>
    /// <returns>Disposable Wrapper to the pooled object</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ObjectInstance LeaseObject()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_pool.TryPop(out var value))
            {
                return new ObjectInstance(this, value);
            }
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
        lock (_sync)
        {
            if (_disposed)
            {
                SafeDispose(value);
                return;
            }
            try
            {
                _returnObject?.Invoke(value);
            }
            catch (Exception ex)
            {
                _logError?.Invoke(ex);
                SafeDispose(value);
                return;
            }
            _pool.Push(value);
        }
    }

    /// <summary>
    /// Pooled object instance wrapper
    /// </summary>
    public ref struct ObjectInstance
    {
        private readonly DisposableObjectPool<T> _owner;
        private T? _instance;
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectInstance"/> class.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="instance"></param>
        internal ObjectInstance(DisposableObjectPool<T> owner, T instance)
        {
            _owner = owner;
            _instance = instance;
        }

        /// <summary>
        /// Gets the instance of the pooled object
        /// </summary>
        public T Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _instance ?? throw new ObjectDisposedException(nameof(ObjectInstance));
        }

        /// <summary>
        /// Returns the object to the pool
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var instance = Interlocked.Exchange<T?>(ref _instance, null);
            if (instance != null)
            {
                _owner.ReturnObject(instance);
            }
        }
    }
}
