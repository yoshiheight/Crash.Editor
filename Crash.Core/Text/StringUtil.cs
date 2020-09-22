using System;
using System.Linq;

namespace Crash.Core.Text
{
    /// <summary>
    /// 文字列ユーティリティー。
    /// </summary>
    public static class StringUtil
    {
        /// <summary>ダブルクォーテーション。</summary>
        public const string DoubleQuote = "\"";

        /// <summary>全角スペースを表す。</summary>
        public const string FullWidthSpace = "　";

        /// <summary>半角スペースを表す。</summary>
        public const string Space = " ";

        /// <summary>タブ文字を表す。</summary>
        public const string Tab = "\t";

        /// <summary>キャリッジリターンを表す。</summary>
        public const string Cr = "\r";

        /// <summary>ラインフィードを表す。</summary>
        public const string Lf = "\n";

        /// <summary>キャリッジリターン＋ラインフィードを表す。</summary>
        public const string CrLf = "\r\n";

        /// <summary>
        /// 文字列の等価性をバイナリで判定する。
        /// </summary>
        public static bool EqualsOrdinal(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.Ordinal);
        }

        /// <summary>
        /// 文字列の等価性をバイナリで判定する（大文字小文字は無視）。
        /// </summary>
        public static bool EqualsOrdinalIgnoreCase(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 文字列をバイナリで大小比較する。
        /// </summary>
        public static int CompareOrdinal(string str1, string str2)
        {
            return string.Compare(str1, str2, StringComparison.Ordinal);
        }

        /// <summary>
        /// 文字列をバイナリで大小比較する（大文字小文字は無視）。
        /// </summary>
        public static int CompareOrdinalIgnoreCase(string str1, string str2)
        {
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 単一行かどうか。
        /// </summary>
        public static bool IsSingleLine(string text)
        {
            return !text.Any(ch => CharUtil.IsNewLine(ch));
        }
    }
}
