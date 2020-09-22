using System;
using System.Threading;

namespace Crash.Core
{
    /// <summary>
    /// 
    /// </summary>
    [ThreadSafe]
    public sealed class Disposable : IDisposable
    {
        private volatile Action? _action;

        internal Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null)?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public static Disposable Create(Action action)
        {
            return new Disposable(action);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Disposable<TValue> Create<TValue>(TValue value, Action<TValue> action)
        {
            return new Disposable<TValue>(value, action);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [ThreadSafe]
    public sealed class Disposable<TValue> : IDisposable
    {
        private volatile Action<TValue>? _action;

        public TValue Value { get; }

        internal Disposable(TValue value, Action<TValue> action)
        {
            Value = value;
            _action = action;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null)?.Invoke(Value);
        }
    }
}
