using System;
using System.Collections.Generic;

namespace Crash.Core.Collections
{
    /// <summary>
    /// ギャップバッファ。
    /// </summary>
    public sealed class GapBuffer<TElement> : IListEx<TElement>
    {
        private readonly InternalGapBuffer<TElement> _internalBuffer;

        public int Capacity => _internalBuffer.Capacity;

        /// <summary>要素数。</summary>
        public int Count => _internalBuffer.Count;

        /// <summary>
        /// 
        /// </summary>
        public GapBuffer(int initialCapacity = 1024)
        {
            _internalBuffer = new InternalGapBuffer<TElement>(initialCapacity);
        }

        /// <summary>インデクサ。</summary>
        public TElement this[int index]
        {
            get => _internalBuffer[index];
            set => _internalBuffer[index] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Insert(int index, TElement element)
        {
            _internalBuffer.MoveGap(index);
            _internalBuffer.Insert(element);
        }

        /// <summary>
        /// 任意の位置に要素を複数挿入する。
        /// </summary>
        public void InsertRange(int index, IEnumerable<TElement> elements)
        {
            _internalBuffer.MoveGap(index);
            foreach (var element in elements)
            {
                _internalBuffer.Insert(element);
            }
        }

        /// <summary>
        /// 任意の位置に要素を複数挿入する。
        /// </summary>
        public void InsertRange(int index, ReadOnlySpan<TElement> elements)
        {
            _internalBuffer.MoveGap(index);
            _internalBuffer.InsertRange(elements);
        }

        /// <summary>
        /// 要素を複数追加する。
        /// </summary>
        public void AddRange(IEnumerable<TElement> elements)
        {
            InsertRange(Count, elements);
        }

        /// <summary>
        /// 要素を複数追加する。
        /// </summary>
        public void AddRange(ReadOnlySpan<TElement> elements)
        {
            InsertRange(Count, elements);
        }

        /// <summary>
        /// 末尾に要素を追加する。
        /// </summary>
        public void Add(TElement element)
        {
            Insert(Count, element);
        }

        /// <summary>
        /// 任意の位置の要素を削除する。
        /// </summary>
        public void RemoveAt(int index)
        {
            _internalBuffer.MoveGap(index);
            _internalBuffer.Remove();
        }

        /// <summary>
        /// 任意の範囲の要素を削除する。
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            _internalBuffer.MoveGap(index);
            _internalBuffer.RemoveRange(count);
        }

        public TElement[] SliceToArray(int index, int count)
        {
            var array = new TElement[count];
            _internalBuffer.CopyTo(index, count, array.AsSpan());
            return array;
        }

        /// <summary>
        /// 全ての要素を削除する。
        /// </summary>
        public void Clear()
        {
            _internalBuffer.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void TrimExcess()
        {
            if (Count < (Capacity * 0.9))
            {
                _internalBuffer.Reallocate(Count);
            }
        }
    }
}
