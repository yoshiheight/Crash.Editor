using System.Text;

namespace Crash.Core.Text
{
    /// <summary>
    /// 文字列エンコード一覧。
    /// </summary>
    public static class TextEncodes
    {
        /// <summary>UTF-8 BOM無しエンコーディング。</summary>
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);
    }
}
