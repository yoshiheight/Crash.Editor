using System;
using System.Threading;
using Crash.Core.Threading;

namespace Crash.Core.UI.Common
{
    /// <summary>
    /// GUI環境で使用するタイマー。
    /// タイマー停止後であればDispose()を呼び出す必要はない。
    /// </summary>
    public sealed class UITimer : IDisposable
    {
        private readonly SynchronizationContext? _sc = SynchronizationContext.Current;

        private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

        private readonly int _intervalMilliseconds;

        private readonly bool _autoReset;

        private readonly Action _callback;

        private System.Threading.Timer? _timer;

        private object? _cookie;

        public bool Enabled => _cookie != null;

        /// <summary>
        /// 
        /// </summary>
        public UITimer(int intervalMilliseconds, bool autoReset, Action callback)
        {
            Verifier.Verify<ArgumentException>(intervalMilliseconds > 0);

            _intervalMilliseconds = intervalMilliseconds;
            _autoReset = autoReset;
            _callback = callback;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Verifier.Verify<InvalidOperationException>(_cookie == null);
            Verifier.Verify<InvalidOperationException>(_timer == null);

            _cookie = new object();
            var state = new State(this, _cookie);
            _timer = new System.Threading.Timer(OnTimer, state, _intervalMilliseconds, Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            _cookie = null;
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnTimer(object stateObj)
        {
            _sc.PostOrInvoke(s =>
            {
                var state = (State)s;
                state.Self.OnTimerOnUIThread(state.Cookie);
            }, stateObj);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnTimerOnUIThread(object cookieState)
        {
            if (!object.ReferenceEquals(_cookie, cookieState)) return;

            // Blazor WebAssemblyの場合は同期コンテキストは取得できないが、タイマーイベントはUIスレッドで発生すると想定
            Verifier.Verify<NotSupportedException>(_threadId == Thread.CurrentThread.ManagedThreadId);

            if (_autoReset)
            {
                _timer!.Change(_intervalMilliseconds, Timeout.Infinite);
            }
            else
            {
                Stop();
            }

            _callback();
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class State
        {
            public UITimer Self { get; }

            public object Cookie { get; }

            public State(UITimer self, object cookie) => (Self, Cookie) = (self, cookie);
        }
    }
}
