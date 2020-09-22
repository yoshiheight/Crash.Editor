using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Crash.Core
{
    /// <summary>
    /// オブジェクト用拡張メソッド。
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// アップキャスト。用途としては、明示的インターフェイスの呼び出しに使用する等。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T UpCast<T>(this T obj)
            where T : class?
        {
            return obj;
        }

        /// <summary>
        /// ダウンキャスト。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DownCast<T>(this object obj)
            where T : class?
        {
            return (T)obj;
        }

        /// <summary>
        /// ボックス化。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Boxing<TSource, TResult>(this TSource source, out TResult outResult)
            where TSource : struct
            where TResult : class
        {
            outResult = (TResult)(object)source;
        }

        /// <summary>
        /// ボックス化。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Boxing<TSource, TResult>(this TSource? source, [NotNullIfNotNull("source")] out TResult? outResult)
            where TSource : struct
            where TResult : class
        {
            outResult = (TResult?)(object?)source;
        }

        /// <summary>
        /// ボックス化解除。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unboxing<TSource, TResult>(this TSource source, out TResult outResult)
            where TSource : class
            where TResult : struct
        {
            outResult = (TResult)(object)source;
        }

        /// <summary>
        /// ボックス化解除。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unboxing<TSource, TResult>(this TSource? source, [NotNullIfNotNull("source")] out TResult? outResult)
            where TSource : class
            where TResult : struct
        {
            outResult = (TResult?)(object?)source;
        }

        /// <summary>
        /// 親と自分を列挙。
        /// </summary>
        public static IEnumerable<T> GetParentsAndSelf<T>(this T source, bool inclusiveSelf, Func<T, T?> parentGetter)
            where T : class
        {
            if (inclusiveSelf)
            {
                yield return source;
            }

            T? element = source;
            while ((element = parentGetter(element)) != null)
            {
                yield return element;
            }
        }
    }
}
