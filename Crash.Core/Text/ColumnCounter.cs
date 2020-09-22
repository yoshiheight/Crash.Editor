namespace Crash.Core.Text
{
    public sealed class ColumnCounter
    {
        private readonly int _tabCount;

        /// <summary>追加した文字のカラム幅の合計。</summary>
        public int TotalColumnCount { get; private set; }

        /// <summary>最後に追加した文字のカラム幅（半角なら1、全角なら2、タブ文字ならタブ幅）。</summary>
        public int LastColumnCount { get; private set; }

        /// <summary>最後に追加した文字の文字数（半角or全角なら1、タブ文字ならタブ幅）。</summary>
        public int LastCharCount { get; private set; }

        public ColumnCounter(int tabCount)
        {
            _tabCount = tabCount;
        }

        public void Add(char ch)
        {
            LastColumnCount =
                (ch == CharUtil.Tab) ? _tabCount - (TotalColumnCount % _tabCount)
                : CharUtil.IsHalfWidth(ch) ? 1
                : 2;
            TotalColumnCount += LastColumnCount;
            LastCharCount = ch == CharUtil.Tab ? LastColumnCount : 1;
        }
    }
}
