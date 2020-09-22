using System;
using Crash.Core.Buffers;

namespace Crash.Core.Collections
{
    /// <summary>
    /// ギャップバッファとしての必要最小限の機能を提供する。
    /// </summary>
    internal sealed class InternalGapBuffer<TElement>
    {
        /// <summary>初期バッファサイズ。</summary>
        private readonly int _initialCapacity;

        /// <summary>ギャップ範囲。</summary>
        private readonly GapRange _gapRange;

        /// <summary>バッファ。</summary>
        private TElement[] _buffer;

        /// <summary>バッファサイズ。</summary>
        public int Capacity => _buffer.Length;

        /// <summary>要素数。</summary>
        public int Count => Capacity - _gapRange.Length;

        private int ForwardCount => _gapRange.StartIndex;

        private int BackwardIndex => _gapRange.EndIndex;

        private int BackwardCount => Capacity - BackwardIndex;

        private Span<TElement> AsGapSpan() => _buffer.AsSpan()[_gapRange.AsRange()];

        private Span<TElement> AsForwardSpan() => _buffer.AsSpan()[..ForwardCount];

        private Span<TElement> AsBackwardSpan() => _buffer.AsSpan()[BackwardIndex..];

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public InternalGapBuffer(int initialCapacity)
        {
            _initialCapacity = initialCapacity;
            _gapRange = new GapRange(initialCapacity);
            _buffer = new TElement[initialCapacity];
        }

        /// <summary>インデクサ。</summary>
        public TElement this[int index]
        {
            get => _buffer[ToIndexInBuffer(index)];
            set => _buffer[ToIndexInBuffer(index)] = value;
        }

        /// <summary>
        /// バッファ内に格納されている実際の要素インデックスに変換する。
        /// </summary>
        private int ToIndexInBuffer(int index)
        {
            Verifier.VerifyIndex(index, Count);

            return (index < _gapRange.StartIndex) ? index : index + _gapRange.Length;
        }

        /// <summary>
        /// 現在位置に要素を挿入し、現在位置をその分進める。
        /// </summary>
        public void Insert(TElement element)
        {
            if (_gapRange.IsEmpty)
            {
                Reallocate(Capacity * 2);
            }
            _buffer[_gapRange.StartIndex] = element;
            _gapRange.StartIndex++;
            _gapRange.Length--;
        }

        /// <summary>
        /// 現在位置に要素配列を挿入し、現在位置をその分進める。
        /// </summary>
        public void InsertRange(ReadOnlySpan<TElement> elements)
        {
            if (elements.Length > _gapRange.Length)
            {
                Reallocate(Math.Max(Capacity, elements.Length) * 2);
            }
            elements.CopyTo(AsGapSpan());
            _gapRange.StartIndex += elements.Length;
            _gapRange.Length -= elements.Length;
        }

        /// <summary>
        /// 現在位置から要素を削除する。
        /// </summary>
        public void Remove()
        {
            Verifier.Verify<ArgumentOutOfRangeException>(BackwardCount > 0);

            _buffer[BackwardIndex] = default!;
            _gapRange.Length++;
        }

        /// <summary>
        /// 現在位置から指定個数の要素を削除する。
        /// </summary>
        public void RemoveRange(int count)
        {
            Verifier.Verify<ArgumentOutOfRangeException>(count <= BackwardCount);

            AsBackwardSpan()[..count].Clear();
            _gapRange.Length += count;
        }

        /// <summary>
        /// 全ての要素を削除する。
        /// </summary>
        public void Clear()
        {
            AsForwardSpan().Clear();
            AsBackwardSpan().Clear();
            _gapRange.StartIndex = 0;
            _gapRange.Length = Capacity;
        }

        /// <summary>
        /// 現在位置を移動する。
        /// </summary>
        public void MoveGap(int movedIndex)
        {
            // 現在 ■■■□□□□■■■■■■■
            // 前へ ■□□□□■■■■■■■■■
            // 後へ ■■■■■■■■■□□□□■

            Verifier.VerifyIndexAllowEnd(movedIndex, Count);
            if (movedIndex == _gapRange.StartIndex)
            {
                return;
            }
            if (_gapRange.IsEmpty)
            {
                _gapRange.StartIndex = movedIndex;
                return;
            }

            var isForward = movedIndex < _gapRange.StartIndex;
            var start = isForward ? movedIndex : _gapRange.EndIndex;
            var length = Math.Abs(movedIndex - _gapRange.StartIndex);
            var offset = _gapRange.Length * (isForward ? 1 : -1);
            _buffer.AsSpan().Move(start, length, offset);
            _gapRange.StartIndex = movedIndex;
        }

        /// <summary>
        /// 現在位置のままバッファを増減する。
        /// </summary>
        public void Reallocate(int newCapacity)
        {
            // 現在 ■■■□□□□□■■■■■
            // 増加 ■■■□□□□□□□■■■■■
            // 減少 ■■■□□□■■■■■

            newCapacity = MathUtil.ClampMin(newCapacity, _initialCapacity);
            if (newCapacity == Capacity)
            {
                return;
            }
            Verifier.Verify<InvalidOperationException>(newCapacity >= Count);

            var newArray = new TElement[newCapacity];
            var offsetCapacity = newCapacity - Capacity;

            AsForwardSpan().CopyTo(newArray.AsSpan());
            AsBackwardSpan().CopyTo(newArray.AsSpan().Slice(BackwardIndex + offsetCapacity));

            _buffer = newArray;
            _gapRange.Length += offsetCapacity;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CopyTo(int start, int length, Span<TElement> dstSpan)
        {
            Verifier.VerifyRangeAllowEmpty(start, length, Count);

            var fwSpan = AsForwardSpan().SliceSafely(start, length);
            start += fwSpan.Length;
            length -= fwSpan.Length;
            var bwSpan = AsBackwardSpan().SliceSafely(start - ForwardCount, length);

            fwSpan.CopyTo(dstSpan);
            dstSpan = dstSpan.Slice(fwSpan.Length);
            bwSpan.CopyTo(dstSpan);
        }

        /// <summary>
        /// ギャップ範囲。
        /// </summary>
        private sealed class GapRange
        {
            /// <summary>位置。</summary>
            public int StartIndex { get; set; }

            /// <summary>長さ。</summary>
            public int Length { get; set; }

            /// <summary>末尾位置。</summary>
            public int EndIndex => StartIndex + Length;

            public Range AsRange() => new Range(StartIndex, EndIndex);

            /// <summary>ギャップが空かどうか。</summary>
            public bool IsEmpty => Length == 0;

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public GapRange(int initialCapacity)
            {
                Length = initialCapacity;
            }
        }
    }
}
