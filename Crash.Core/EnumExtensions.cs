using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Crash.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static string GetStringValue<TEnum>(this TEnum enumValue)
            where TEnum : Enum
        {
            return EnumAttributeGetter<StringValueAttribute>.Get(enumValue).Value;
        }
    }

    /// <summary>
    /// enumフィールドに文字列値を設定可能にする属性。
    /// 値としての文字列としてのみ使用し、表示用文字列としては使用しないこと。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringValueAttribute : Attribute
    {
        public string Value { get; }

        public StringValueAttribute(string value) => Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class EnumAttributeGetter<TAttribute>
        where TAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public static IReadOnlyList<TAttribute> Gets<TEnum>(TEnum enumValue)
            where TEnum : Enum
        {
            return EnumAttributeCache<TEnum, TAttribute>.GetAttributes(enumValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public static TAttribute Get<TEnum>(TEnum enumValue)
            where TEnum : Enum
        {
            return Gets(enumValue).Single();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class EnumAttributeCache<TEnum, TAttribute>
        where TEnum : Enum
        where TAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<TEnum, IReadOnlyList<TAttribute>> __cache = new ConcurrentDictionary<TEnum, IReadOnlyList<TAttribute>>();

        /// <summary>
        /// 
        /// </summary>
        public static IReadOnlyList<TAttribute> GetAttributes(TEnum enumValue)
        {
            return __cache.GetOrAdd(enumValue, key => getAttributes(key));

            // 属性の取得
            static IReadOnlyList<TAttribute> getAttributes(TEnum enumValue)
            {
                var enumFieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                return enumFieldInfo.GetCustomAttributes<TAttribute>(false).ToArray();
            }
        }
    }
}
