using System.Collections.Generic;
using Crash.Core.Collections;

namespace Crash.Core.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public static class RectListExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static void UnionLastContinuousOrAdd(this IList<Rect2D> rectList, Rect2D rect)
        {
            if (rectList.LastOrNullValue() is { } last && last.Inflate(1, 1).IntersectsWith(rect))
            {
                rectList[^1] = last.Union(rect);
            }
            else
            {
                rectList.Add(rect);
            }
        }
    }
}
