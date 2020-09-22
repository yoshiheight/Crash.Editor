using System;

namespace Crash.Core
{
    /// <summary>
    /// 不変クラスに指定する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ImmutableAttribute : Attribute { }

    /// <summary>
    /// スレッドセーフなものに指定する属性。
    /// （静的なものは元々スレッドセーフにしておくことが前提なので、この属性は指定しないこと）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ThreadSafeAttribute : Attribute { }

    /// <summary>
    /// 非スレッドセーフな静的なものに指定する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NonThreadSafeAttribute : Attribute { }

    /// <summary>
    /// 何もしないメソッドに指定する属性。
    /// 例）protected virtualな空実装メソッドで、派生クラスでoverrideした場合に基底クラス側を呼ばなくていい場合
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NoopAttribute : Attribute { }

    /// <summary>
    /// 破壊的変更の対象となるメソッド引数に指定する属性。
    /// （破壊的変更自体はよくないので特別な理由がない限りは行わないこと）
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ModifyAttribute : Attribute { }

    /// <summary>
    /// 集約のフィールドに指定する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AggregationAttribute : Attribute { }
}
