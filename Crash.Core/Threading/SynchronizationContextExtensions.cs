using System.Threading;

namespace Crash.Core.Threading
{
    public static class SynchronizationContextExtensions
    {
        public static void PostOrInvoke(this SynchronizationContext? sc, SendOrPostCallback callback, object state)
        {
            if (sc != null)
            {
                sc.Post(callback, state);
            }
            else
            {
                callback(state);
            }
        }
    }
}
