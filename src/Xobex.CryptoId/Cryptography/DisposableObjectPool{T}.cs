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
/// <remarks>
/// Pool growth limited by maxSize parameter
/// </remarks>
internal sealed class DisposableObjectPool<T> : IDisposable
    where T : class, IDisposable
{
    private readonly Lock _sync = new();
    private readonly Stack<T> _pool = [];

    private readonly Func<T> _createObject;
    private readonly Action<Exception>? _logError;
    private readonly int _maxSize;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableObjectPool{T}"/> class.
    /// </summary>
    /// <param name="createObject">Factory function used to create pooled objects.</param>
    /// <param name="logError">Callback invoked when disposing a pooled object throws; may be null.</param>
    /// <param name="maxSize">Maximum number of objects to pool (default: 10). Objects beyond this limit are disposed immediately.</param>
    public DisposableObjectPool(Func<T> createObject, Action<Exception>? logError = null, int maxSize = 10)
    {
        _createObject = createObject ?? throw new ArgumentNullException(nameof(createObject));
        _logError = logError;
        _maxSize = maxSize > 0 ? maxSize : throw new ArgumentException("maxSize must be greater than 0", nameof(maxSize));
    }

    /// <summary>
    /// Disposes the pool and all pooled objects
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
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
    /// <remarks>
    /// Always use this pattern
    /// <code>
    ///     using var lease = _pool.LeaseObject();
    ///     lease.Instance.EncryptEcb(block, encryptedBlock, PaddingMode.None);
    /// </code>
    /// Never copy `lease` ref struct
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ObjectInstance LeaseObject()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
    /// <param name="value">returned object</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnObject(T value)
    {
        if (_disposed)
        {
            SafeDispose(value);
            return;
        }
        lock (_sync)
        {
            if (_disposed)
            {
                SafeDispose(value);
                return;
            }
            if (_pool.Count < _maxSize)
            {
                _pool.Push(value);
            }
            else
            {
                SafeDispose(value);
            }
        }
    }

    /// <summary>
    /// Pooled object instance wrapper
    /// </summary>
    internal readonly ref struct ObjectInstance
    {
        private readonly DisposableObjectPool<T> _owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectInstance"/> struct.
        /// </summary>
        /// <param name="owner">The pool that owns the instance.</param>
        /// <param name="instance">The pooled object instance.</param>
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
