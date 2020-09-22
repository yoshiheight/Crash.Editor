using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Crash.Core.Collections
{
    public static class CollectionExtensions
    {
        public static TValue GetOrAdd<TKey, TValue, TArg>(
            this IDictionary<TKey, TValue> source,
            TKey key, Func<TKey, TArg, TValue> valueFactory, in TArg arg)
            where TKey : notnull
        {
            if (source.TryGetValue(key, out var value))
            {
                return value;
            }
            return source[key] = valueFactory(key, arg);
        }

        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key, Func<TKey, TValue> valueFactory)
            where TKey : notnull
        {
            if (source.TryGetValue(key, out var value))
            {
                return value;
            }
            return source[key] = valueFactory(key);
        }

        [return: MaybeNull]
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
            where TKey : notnull
        {
            if (source.TryGetValue(key, out var value))
            {
                return value;
            }
            return default;
        }

        public static TValue? GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
            where TKey : notnull
            where TValue : class
        {
            if (source.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public static TValue? GetValueOrNullValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
            where TKey : notnull
            where TValue : struct
        {
            if (source.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
