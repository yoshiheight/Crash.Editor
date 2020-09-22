using System;

namespace Crash.Core.Buffers
{
    /// <summary>
    /// 
    /// </summary>
    public static class RangeExtensions
    {
        /// <summary>
        /// 範囲をオフセットして返す。
        /// </summary>
        public static Range Offset(this Range range, int length, int offset)
        {
            var item = range.GetOffsetAndLength(length);
            return new Range(item.Offset + offset, item.Offset + offset + item.Length);
        }
    }
}
