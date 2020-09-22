using System.Collections.Generic;
using System.IO;
using Crash.Core.Collections;

namespace Crash.Core.Text
{
    /// <summary>
    /// 
    /// </summary>
    public static class TextReaderWriterExtensions
    {
        /// <summary>
        /// 一文字読み込む。
        /// 改行だった場合のみ、戻り値の改行文字列が非nullとなる。
        /// 改行コードは「CR」「LF」「CRLF」「それらの混在」に対応。
        /// </summary>
        public static (char ch, string? newLine)? ReadChar(this TextReader reader)
        {
            var code = reader.Read();
            if (code != -1)
            {
                var ch = (char)code;
                string? newLine = null;
                if (ch == CharUtil.Lf)
                {
                    newLine = StringUtil.Lf;
                }
                else if (ch == CharUtil.Cr)
                {
                    code = reader.Peek();
                    if (code != -1 && (char)code == CharUtil.Lf)
                    {
                        ch = (char)reader.Read();
                        newLine = StringUtil.CrLf;
                    }
                    else
                    {
                        newLine = StringUtil.Cr;
                    }
                }
                return (ch, newLine);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void WriteLines(this TextWriter writer, IEnumerable<string> lines, string newLine)
        {
            foreach (var line in lines.Separate(newLine))
            {
                writer.Write(line);
            }
        }
    }
}
