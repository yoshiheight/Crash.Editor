using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Crash.Core.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool IsEmpty<TElem>(this IEnumerable<TElem> source)
        {
            return !source.Any();
        }

        /// <summary>
        /// シーケンスの各要素にインデックスを付加する。
        /// </summary>
        public static IEnumerable<(TElem value, int index)> Indexed<TElem>(this IEnumerable<TElem> source)
        {
            return source.Select((value, index) => (value, index));
        }

        /// <summary>
        /// 
        /// </summary>
        public static TElem? FirstOrNull<TElem>(this IEnumerable<TElem> source)
            where TElem : class
        {
            return source.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        public static TElem? FirstOrNullValue<TElem>(this IEnumerable<TElem> source)
            where TElem : struct
        {
            return source.TryFirst(out var elem) ? elem : (TElem?)null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool TryFirst<TElem>(this IEnumerable<TElem> source, [MaybeNullWhen(false)] out TElem outElem)
        {
            outElem = default!;

            if (source is IList<TElem> list)
            {
                if (list.Count > 0)
                {
                    outElem = list[0];
                    return true;
                }
                return false;
            }

            foreach (var elem in source)
            {
                outElem = elem;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static TElem? LastOrNull<TElem>(this IEnumerable<TElem> source)
            where TElem : class
        {
            return source.LastOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        public static TElem? LastOrNullValue<TElem>(this IEnumerable<TElem> source)
            where TElem : struct
        {
            return source.TryLast(out var elem) ? elem : (TElem?)null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool TryLast<TElem>(this IEnumerable<TElem> source, [MaybeNullWhen(false)] out TElem outElem)
        {
            outElem = default!;

            if (source is IList<TElem> list)
            {
                if (list.Count > 0)
                {
                    outElem = list[^1];
                    return true;
                }
                return false;
            }

            var any = false;
            foreach (var elem in source)
            {
                outElem = elem;
                any = true;
            }
            return any;
        }

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<TElem> Separate<TElem>(this IEnumerable<TElem> source, TElem separator)
        {
            var count = 0;
            foreach (var elem in source)
            {
                if (count++ > 0)
                {
                    yield return separator;
                }
                yield return elem;
            }
        }

        /// <summary>
        /// 各要素から取得したサイズ値の合計が上限値以上になるまでの間を1グループとして、グループ化を行う。
        /// 上限値以上になった時点の要素は、それより手前のグループに含める。
        /// </summary>
        public static IEnumerable<IGrouping<int, TElement>> Chunk<TElement>(
            this IEnumerable<TElement> source,
            Func<TElement, int> sizeGetter,
            int limitSize)
        {
            var key = 0;
            int totalSize = 0;
            return source
                .Select(elem =>
                {
                    if (totalSize >= limitSize)
                    {
                        totalSize = 0;
                        key++;
                    }
                    totalSize += sizeGetter(elem);
                    return (key, elem);
                })
                .GroupBy(item => item.key, item => item.elem);
        }

        /// <summary>
        /// シーケンスを2個ずつに分割する。
        /// 余りが出る場合は例外発生。
        /// </summary>
        public static IEnumerable<TResult> DivideSelect<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> selector)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return selector(enumerator.Current, MoveNextAndGet(enumerator));
                }
            }
        }

        /// <summary>
        /// シーケンスを3個ずつに分割する。
        /// 余りが出る場合は例外発生。
        /// </summary>
        public static IEnumerable<TResult> DivideSelect<T, TResult>(this IEnumerable<T> source, Func<T, T, T, TResult> selector)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return selector(enumerator.Current, MoveNextAndGet(enumerator), MoveNextAndGet(enumerator));
                }
            }
        }

        /// <summary>
        /// 列挙子の現在位置を進め、現在値を取得する。
        /// </summary>
        private static T MoveNextAndGet<T>(IEnumerator<T> enumerator)
        {
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }
            return enumerator.Current;
        }

        /// <summary>
        /// シーケンス内でキーが重複する要素をグループ化して抽出する
        /// </summary>
        public static IEnumerable<IGrouping<TKey, TSource>> Duplicate<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source
                .GroupBy(keySelector)
                .Where(group => group.Skip(1).Any());
        }

        /// <summary>
        /// シーケンス内で重複するキーを抽出する
        /// </summary>
        public static IEnumerable<TKey> DuplicateKey<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source
                .Duplicate(keySelector)
                .Select(group => group.Key);
        }
    }
}
