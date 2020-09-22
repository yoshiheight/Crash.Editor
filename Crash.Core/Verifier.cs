using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Crash.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class Verifier
    {
        /// <summary>
        /// 
        /// </summary>
        public static void Verify<TException>([DoesNotReturnIf(false)] bool condition,
            [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
            where TException : Exception, new()
        {
            if (!condition)
            {
                // Blazor WebAssemblyだとログにエラー箇所が表示されない。
                // 例外名と例外メッセージは表示されるので、ここでエラー箇所の情報を含めておく。
                throw (Exception)Activator.CreateInstance(typeof(TException), $"at {member} in {Path.GetFileName(file)}({line})");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void VerifyIndex(int index, int maxCount,
            [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            Verify<ArgumentOutOfRangeException>(maxCount > 0,
                file, line, member);
            Verify<ArgumentOutOfRangeException>(MathUtil.IsInRange(index, 0, maxCount - 1),
                file, line, member);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void VerifyIndexAllowEnd(int index, int maxCount,
            [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            Verify<ArgumentOutOfRangeException>(MathUtil.IsInRange(index, 0, maxCount),
                file, line, member);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void VerifyRangeAllowEmpty(int start, int length, int maxCount,
            [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            Verify<ArgumentOutOfRangeException>(length >= 0,
                file, line, member);
            VerifyIndexAllowEnd(start, maxCount,
                file, line, member);
            VerifyIndexAllowEnd(start + length, maxCount,
                file, line, member);
        }
    }
}
