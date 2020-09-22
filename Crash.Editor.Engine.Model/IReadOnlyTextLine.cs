namespace Crash.Editor.Engine.Model
{
    /// <summary>
    /// 行を表す。
    /// </summary>
    public interface IReadOnlyTextLine
    {
        string Text { get; }

        int Length { get; }

        int EndCharIndex { get; }

        bool IsEmpty { get; }

        LineModifyStatus ModifyStatus { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum LineModifyStatus
    {
        None,
        Modified,
        ModifySaved,
    }
}
