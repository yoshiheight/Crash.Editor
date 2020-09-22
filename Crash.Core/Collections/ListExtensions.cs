using System.Collections.Generic;

namespace Crash.Core.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<T> ReverseList<T>(this IReadOnlyList<T> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                yield return list[^(i + 1)];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int LastIndex<T>(this IReadOnlyList<T> list)
        {
            return list.Count - 1;
        }
    }
}
