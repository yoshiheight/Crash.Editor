using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Crash.Core.Text
{
    /// <summary>
    /// テキストリーダーからの読み込みを行う。
    /// 改行コードは「CR」「LF」「CRLF」「それらの混在」に対応。
    /// StreamReaderだと「改行コードの種類が識別できない」「末尾の改行を認識できない」といった機能不足があるので、その改善用。
    /// </summary>
    public sealed class TextLinesReader : IAutoFieldDisposable
    {
        /// <summary>読み込み元。</summary>
        [DisposableField]
        private readonly TextReader _reader;

        /// <summary></summary>
        private readonly HashSet<string> _newLineSet = new HashSet<string>();

        /// <summary></summary>
        public IEnumerable<string> NewLines => _newLineSet;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TextLinesReader(TextReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> ReadLines()
        {
            var buffer = new StringBuilder();
            while (_reader.ReadChar() is { } result)
            {
                if (result.newLine != null)
                {
                    _newLineSet.Add(result.newLine);

                    yield return buffer.ToString();
                    buffer.Clear();
                }
                else
                {
                    buffer.Append(result.ch);
                }
            }

            yield return buffer.ToString();
        }
    }
}
