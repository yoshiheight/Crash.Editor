using System;
using System.Diagnostics;

namespace Crash.Core.Diagnostics
{
    public static class DebugUtil
    {
        public const string DebugSymbol = "DEBUG";

        [Conditional(DebugSymbol)]
        public static void DebugCode(Action action)
        {
            action();
        }
    }
}
