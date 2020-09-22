using System.Diagnostics;
using System.Text;
using Crash.Core.Diagnostics;

namespace Crash.Core.Text
{
    /// <summary>
    /// 文字ユーティリティー。
    /// </summary>
    public static class CharUtil
    {
        /// <summary>全角スペースを表す。</summary>
        public const char FullWidthSpace = '　';

        /// <summary>半角スペースを表す。</summary>
        public const char Space = ' ';

        /// <summary>アンダースコアを表す。</summary>
        public const char Underscore = '_';

        /// <summary>タブ文字を表す。</summary>
        public const char Tab = '\t';

        /// <summary>キャリッジリターンを表す。</summary>
        public const char Cr = '\r';

        /// <summary>ラインフィードを表す。</summary>
        public const char Lf = '\n';

        /// <summary>NULL文字。</summary>
        public const char Null = '\0';

        /// <summary>ASCII文字範囲</summary>
        private static readonly (char start, char end) AsciiRange = ('\x20', '\x7E');

        /// <summary>半角カタカナ文字範囲</summary>
        private static readonly (char start, char end) HalfKatakanaRange = ('\xFF61', '\xFF9F');

        /// <summary>
        /// 静的コンストラクタ。
        /// </summary>
        static CharUtil()
        {
            // 半角文字判定が正しいかの検証
            DebugUtil.DebugCode(() =>
            {
                var asciiStringBuilder = new StringBuilder();
                foreach (var n in MathUtil.RangeStartToEndInclusive(AsciiRange.start, AsciiRange.end))
                {
                    asciiStringBuilder.Append((char)n);
                }
                var halfKatakanaStringBuilder = new StringBuilder();
                foreach (var n in MathUtil.RangeStartToEndInclusive(HalfKatakanaRange.start, HalfKatakanaRange.end))
                {
                    halfKatakanaStringBuilder.Append((char)n);
                }

                // 半角文字一覧
                // （実際には他にも様々な記号や国の文字で半角のものはあるが、それらは考慮しない事とする）
                // （厳密に半角判定する場合はその時のフォント名とフォントサイズで文字描画の横幅を取得して判断が必要）
                const string AsciiChars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                const string HalfKatakanaChars = "｡｢｣､･ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ";

                Debug.Assert(asciiStringBuilder.Length + halfKatakanaStringBuilder.Length == 158);
                Debug.Assert(asciiStringBuilder.ToString() == AsciiChars);
                Debug.Assert(halfKatakanaStringBuilder.ToString() == HalfKatakanaChars);
                foreach (var ch in AsciiChars)
                {
                    Debug.Assert(IsAscii(ch));
                }
                foreach (var ch in HalfKatakanaChars)
                {
                    Debug.Assert(IsHalfKatakana(ch));
                }
            });
        }

        /// <summary>
        /// 半角文字かどうか。
        /// </summary>
        public static bool IsHalfWidth(char ch)
        {
            return IsAscii(ch) || IsHalfKatakana(ch);
        }

        /// <summary>
        /// ASCII文字かどうか。
        /// </summary>
        public static bool IsAscii(char ch)
        {
            return MathUtil.IsInRange<int>(ch, AsciiRange.start, AsciiRange.end);
        }

        /// <summary>
        /// 半角カタカナ文字かどうか。
        /// </summary>
        public static bool IsHalfKatakana(char ch)
        {
            return MathUtil.IsInRange<int>(ch, HalfKatakanaRange.start, HalfKatakanaRange.end);
        }

        /// <summary>
        /// 描画可能な文字かどうか。
        /// </summary>
        public static bool CanDraw(char ch)
        {
            return !char.IsControl(ch);
        }

        /// <summary>
        /// 改行文字かどうか。
        /// </summary>
        public static bool IsNewLine(char ch)
        {
            return ch == Cr || ch == Lf;
        }

        /// <summary>
        /// 単語に使用する半角文字かどうか。
        /// </summary>
        public static bool IsHalfWidthWord(char ch)
        {
            return IsHalfWidth(ch) && (char.IsLetterOrDigit(ch) || ch == Underscore);
        }

        /// <summary>
        /// 単語に使用する全角文字かどうか。
        /// </summary>
        public static bool IsFullWidthWord(char ch)
        {
            return !IsHalfWidth(ch)
                && !char.IsWhiteSpace(ch)
                && !char.IsPunctuation(ch)
                && !char.IsSeparator(ch)
                && !char.IsSymbol(ch);
        }

        /// <summary>
        /// 文字が同一単語グループかどうか。
        /// </summary>
        public static bool EqualsWordGroup(char ch1, char ch2)
        {
            return ch1 == ch2
                || (char.IsWhiteSpace(ch1) && char.IsWhiteSpace(ch2))
                || (IsHalfWidthWord(ch1) && IsHalfWidthWord(ch2))
                || (IsFullWidthWord(ch1) && IsFullWidthWord(ch2));
        }
    }
}
