using System;

namespace Crash.Core.Buffers
{
    /// <summary>
    /// 
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static void CopyTo<T>(this Span<T> span, Span<T> dstSpan, out int outLength)
        {
            span.CopyTo(dstSpan);
            outLength = span.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public static Range Move<T>(this Span<T> span, int start, int length, int offset)
        {
            return Move(span, new Range(start, start + length), offset);
        }

        /// <summary>
        /// スパン内の指定範囲を移動し、移動後の範囲を返す。
        /// 移動によってできた隙間は要素型のデフォルト値で埋める。
        /// </summary>
        public static Range Move<T>(this Span<T> span, Range range, int offset)
        {
            // 現在       □□□□□□■■■■□□□□
            // 後方へ移動 □□□□□□□□■■■■□□
            // 前方へ移動 □□■■■■□□□□□□□□

            var srcSpan = span[range];
            var dstRange = range.Offset(span.Length, offset);
            var dstSpan = span[dstRange];
            srcSpan.CopyTo(dstSpan);

            var clearLength = Math.Min(srcSpan.Length, Math.Abs(offset));
            var clearRange = (offset >= 0) ? ..clearLength : ^clearLength..;
            srcSpan[clearRange].Clear();

            return dstRange;
        }

        /// <summary>
        /// スライスする。
        /// 指定の開始位置が有効範囲外の場合は空のスパンを返す。
        /// 指定の長さが有効範囲を超える場合はクランプする。
        /// </summary>
        public static Span<T> SliceSafely<T>(this Span<T> span, int start, int length)
        {
            Verifier.Verify<ArgumentOutOfRangeException>(length >= 0);

            return MathUtil.IsInRange(start, 0, span.Length) ?
                span.Slice(start, MathUtil.ClampMax(length, span.Length - start)) : null;
        }
    }
}
