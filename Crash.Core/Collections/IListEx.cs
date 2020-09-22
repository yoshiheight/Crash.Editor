using System;
using System.Collections;
using System.Collections.Generic;

namespace Crash.Core.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public interface IListEx<TElement> : IList<TElement>, IReadOnlyList<TElement>
    {
        bool ICollection<TElement>.IsReadOnly => false;

        /// <summary>
        /// 
        /// </summary>
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            var list = (IList<TElement>)this;
            for (var i = 0; i < list.Count; i++)
            {
                yield return list[i];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        int IList<TElement>.IndexOf(TElement target)
        {
            return FindElement(target);
        }

        /// <summary>
        /// 
        /// </summary>
        bool ICollection<TElement>.Contains(TElement target)
        {
            // LINQにContainsがあるが、それを使うとIList.Containsが呼ばれて結果的に無限ループの可能性があるので使用しなかった
            return FindElement(target) != -1;
        }

        /// <summary>
        /// 
        /// </summary>
        bool ICollection<TElement>.Remove(TElement target)
        {
            // is var の書き方を使ってみたかっただけ
            if (FindElement(target) is var index && index != -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        private int FindElement(TElement target)
        {
            var list = (IList<TElement>)this;
            for (var i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<TElement>.Default.Equals(list[i], target))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
