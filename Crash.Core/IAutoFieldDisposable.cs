using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Crash.Core
{
    /// <summary>
    /// フィールドを自動破棄するデフォルト実装を持つインターフェイス。
    /// </summary>
    public interface IAutoFieldDisposable : IDisposable
    {
        /// <summary>
        /// 破棄処理。
        /// </summary>
        void IDisposable.Dispose()
        {
            DisposeFields(this);
        }

        /// <summary>
        /// フィールドの自動破棄処理。
        /// </summary>
        protected static void DisposeFields(IAutoFieldDisposable obj)
        {
            foreach (var type in obj.GetType().GetBaseTypesAndSelf()
                .Where(type => typeof(IAutoFieldDisposable).IsAssignableFrom(type)))
            {
                foreach (var targetInfo in DisposableTargetInfoCache.GetInfosDeclaredOnly(type))
                {
                    var targetObj = targetInfo.FieldInfo.GetValue(obj);
                    if (targetObj == null)
                    {
                        continue;
                    }

                    if (targetInfo.FieldAttr.DisposeTarget == DisposeTarget.Self)
                    {
                        ((IDisposable)targetObj).Dispose();
                    }
                    else if (targetInfo.FieldAttr.DisposeTarget == DisposeTarget.Elements)
                    {
                        if (targetObj is IList<IDisposable?> list)
                        {
                            Disposer.DisposeAll(list);
                            list.Clear();
                        }
                        else if (targetObj is IDictionary dictionary)
                        {
                            Disposer.DisposeAll((IEnumerable<IDisposable?>)dictionary.Values);
                            dictionary.Clear();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    internal sealed class DisposableTargetInfo
    {
        public FieldInfo FieldInfo { get; }

        public DisposableFieldAttribute FieldAttr { get; }

        public DisposableTargetInfo(FieldInfo fieldInfo, DisposableFieldAttribute fieldAttr) => (FieldInfo, FieldAttr) = (fieldInfo, fieldAttr);
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class DisposableTargetInfoCache
    {
        /// <summary>破棄対象のFieldInfoのキャッシュ用。</summary>
        private static readonly ConcurrentDictionary<Type, IReadOnlyList<DisposableTargetInfo>> __cache = new ConcurrentDictionary<Type, IReadOnlyList<DisposableTargetInfo>>();

        /// <summary>
        /// 
        /// </summary>
        public static IReadOnlyList<DisposableTargetInfo> GetInfosDeclaredOnly(Type type)
        {
            return __cache.GetOrAdd(type, key => CreateTargetInfosDeclaredOnly(key));
        }

        /// <summary>
        /// 破棄対象のメタデータの生成。
        /// </summary>
        private static IReadOnlyList<DisposableTargetInfo> CreateTargetInfosDeclaredOnly(Type type)
        {
            var targetInfos = type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(fi => (fi, attr: fi.GetCustomAttribute<DisposableFieldAttribute>(false)))
                .Where(tuple => tuple.attr != null)
                .Select(tuple => new DisposableTargetInfo(tuple.fi, tuple.attr!))
                .GroupBy(targetInfo => targetInfo.FieldAttr.Group)
                .SelectMany(group => group.OrderBy(targetInfo => targetInfo.FieldAttr.Order))
                .ToArray();

            foreach (var targetInfo in targetInfos)
            {
                var fieldType = targetInfo.FieldInfo.FieldType;
                if (targetInfo.FieldAttr.DisposeTarget == DisposeTarget.Self)
                {
                    if (!typeof(IDisposable).IsAssignableFrom(fieldType))
                    {
                        throw new InvalidDisposableFieldException();
                    }
                }
                else if (targetInfo.FieldAttr.DisposeTarget == DisposeTarget.Elements)
                {
                    if (!typeof(IList<IDisposable?>).IsAssignableFrom(fieldType)
                        && !IsElementDisposableDictionary(fieldType))
                    {
                        throw new InvalidDisposableFieldException();
                    }
                }
            }

            return targetInfos;
        }

        /// <summary>
        /// 指定の型がIDictionary[TKey, TValue]に割り当て可能で、且つTValueがIDisposableに割り当て可能かどうか。
        /// </summary>
        private static bool IsElementDisposableDictionary(Type type)
        {
            var interfaceType = type.GetInterfaces()
                .Append(type)
                .FirstOrDefault(t => typeof(IDictionary<,>).IsAssignableFrom(t.GetGenericTypeDefinitionOrNull()));
            return interfaceType != null && typeof(IDisposable).IsAssignableFrom(interfaceType.GetGenericArguments()[1]);
        }
    }

    /// <summary>
    /// 自動破棄するフィールドに指定する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisposableFieldAttribute : Attribute
    {
        public DisposeTarget DisposeTarget { get; }

        public object? Group { get; set; }

        public int Order { get; set; }

        public DisposableFieldAttribute(DisposeTarget disposeTarget = DisposeTarget.Self)
        {
            DisposeTarget = disposeTarget;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DisposeTarget
    {
        Self,
        Elements,
    }

    /// <summary>
    /// 指定されたフィールドがIDisposableではなかった場合に発生する例外。
    /// </summary>
    public sealed class InvalidDisposableFieldException : Exception { }
}

#if false
    /// <summary>
    /// 破棄コンテキスト情報。
    /// </summary>
    [ThreadSafe]
    public sealed class DisposeContext
    {
        /// <summary></summary>
        internal volatile object? _disposedObject;

        public event Action? BeginDispose;

        public event Action? EndDispose;

        internal void RaiseBeginDispose()
        {
            BeginDispose?.Invoke();
            BeginDispose = null;
        }

        internal void RaiseEndDispose()
        {
            EndDispose?.Invoke();
            EndDispose = null;
        }

        /// <summary>
        /// 破棄済みであれば例外を投げる。
        /// 複数スレッドから同時に破棄される場合、当該メソッドが例外を投げなかったからといって、
        /// その後の処理でオブジェクトが使用可能とは限らないので注意。
        /// </summary>
        public void ThrowIfDisposed()
        {
            if (_disposedObject != null)
            {
                throw new ObjectDisposedException(_disposedObject.GetType().FullName);
            }
        }
    }

    //if (Interlocked.CompareExchange(ref DisposeContext._disposedObject, this, null) == null)
    //{
    //    DisposeContext.RaiseBeginDispose();
    //    DisposeFields(this);
    //    DisposeContext.RaiseEndDispose();
    //}
#endif
