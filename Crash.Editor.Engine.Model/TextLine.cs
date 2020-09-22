namespace Crash.Editor.Engine.Model
{
    /// <summary>
    /// 行を表す。
    /// </summary>
    internal sealed class TextLine : IReadOnlyTextLine
    {
        public const int ToEnd = -1;

        public bool IsModifyCleared { get; private set; }

        public int ModifyCount { get; set; }

        public string Text { get; private set; } = string.Empty;

        public int Length => Text.Length;

        public int EndCharIndex => Length - 1;

        public bool IsEmpty => Length == 0;

        public LineModifyStatus ModifyStatus =>
            (ModifyCount != 0) ? LineModifyStatus.Modified
            : IsModifyCleared ? LineModifyStatus.ModifySaved
            : LineModifyStatus.None;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TextLine() { }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TextLine(string text)
        {
            Text = text;
        }

        public void ClearModifyCount()
        {
            IsModifyCleared |= ModifyCount != 0;
            ModifyCount = 0;
        }

        public void Insert(int index, string value)
        {
            Text = Text.Insert(index, value);
        }

        public void Remove(int index, int count = ToEnd)
        {
            if (count == ToEnd)
            {
                count = Text.Length - index;
            }
            Text = Text.Remove(index, count);
        }

        public string Sub(int index, int count = ToEnd)
        {
            if (count == ToEnd)
            {
                count = Text.Length - index;
            }
            return Text.Substring(index, count);
        }

        public string Cut(int index, int count = ToEnd)
        {
            var result = Sub(index, count);
            Remove(index, count);
            return result;
        }
    }
}
