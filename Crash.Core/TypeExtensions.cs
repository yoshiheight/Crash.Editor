using System;
using System.Collections.Generic;

namespace Crash.Core
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetBaseTypesAndSelf(this Type type)
        {
            return type.GetParentsAndSelf(true, t => t.BaseType);
        }

        public static Type? GetGenericTypeDefinitionOrNull(this Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : null;
        }
    }
}
