using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Crash.Core;
using Crash.Core.Collections;
using Crash.Core.Diagnostics;
using Crash.Core.Drawing;
using Crash.Core.Text;

namespace Crash.Editor.Engine.View.Common.Measurement
{
    /// <summary>
    /// 描画用の各要素を測定する。
    /// </summary>
    public sealed class DrawingMetricsMeasure
    {
        [Aggregation]
        private readonly TextView _textView;

        public DrawingMetricsMeasure(TextView textView)
        {
            _textView = textView;
        }

#warning ICharMetricの実体が同一インスタンスなのをなんとかしたい
        /// <summary>
        /// 文字ループ。
        /// </summary>
        public IEnumerable<ICharMetric> EnumerateCharMetrics(IReadOnlyRegion2D clipRegion)
        {
            foreach (var lineMetric in EnumerateLineMetrics(clipRegion))
            {
                foreach (var charMetric in EnumerateCharMetricsByLine(lineMetric.Index, true, true))
                {
                    Debug.Assert(CharUtil.CanDraw(charMetric.Char));

                    if (clipRegion.IntersectsWith(charMetric.CharRect))
                    {
                        yield return charMetric;
                    }

                    if (charMetric.Right > clipRegion.Bounds.Right)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 指定行の指定X座標に該当する文字インデックスを算出する。
        /// </summary>
        public int CalcCharIndex(int lineIndex, int x)
        {
            var totalWidth = 0.0;
            foreach (var metric in EnumerateCharMetricsByLine(lineIndex, false, false))
            {
                var old = totalWidth;
                totalWidth += metric.Width;
                if (totalWidth >= x)
                {
                    return metric.IndexEachLine + ((MathUtil.Magnet(x, old, totalWidth) == old) ? 0 : 1);
                }
            }
            return _textView.Doc.Lines[lineIndex].Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CalcTotalWidth(int lineIndex, int charLength, out double totalWidth, out int totalColumnCount)
        {
            totalWidth = 0.0;
            totalColumnCount = 0;
            foreach (var metric in EnumerateCharMetricsByLine(lineIndex, false, false)
                .Take(charLength))
            {
                totalWidth += metric.Width;
                totalColumnCount += metric.ColumnLen;
            }
        }

#warning ILineMetricの実体が同一インスタンスなのをなんとかしたい
        /// <summary>
        /// 行ループ。
        /// </summary>
        public IEnumerable<ILineMetric> EnumerateLineMetrics(IReadOnlyRegion2D clipRegion)
        {
            var lineHeight = _textView.GetLineHeight();
            var lineIndex = clipRegion.Bounds.Y / lineHeight;
            var y = lineIndex * lineHeight;
            var lineMetric = new LineMetric();
            while (lineIndex < _textView.Doc.Lines.Count)
            {
                if ((y + lineHeight) > clipRegion.Bounds.Top)
                {
                    yield return lineMetric.Init(lineIndex, y, lineHeight);
                }

                lineIndex++;
                y += lineHeight;

                if (y >= clipRegion.Bounds.Bottom)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerable<ICharMetric> EnumerateCharMetricsByLine(int lineIndex, bool isTabProcess, bool isAddMetaChar)
        {
            var chars = _textView.Doc.Lines[lineIndex].Text.AsEnumerable();

            if (isAddMetaChar)
            {
                if (lineIndex < _textView.Doc.Lines.LastIndex())
                {
                    chars = chars.Append(CharUtil.Space);
                }

                DebugUtil.DebugCode(() =>
                {
                    if (lineIndex < _textView.Doc.Lines.LastIndex())
                    {
                        chars = chars.SkipLast(1).Append('↓');
                    }
                    else
                    {
                        chars = chars.Concat("[EOF]");
                    }
                });
            }

            var lineHeight = _textView.GetLineHeight();

            var columnCounter = new ColumnCounter(_textView._settings.tabWidth);
            var originalIndex = 0;
            var index = 0;
            var lineLength = _textView.Doc.Lines[lineIndex].Length;
            var x = 0.0;
            var chItem = new CharMetric();
            foreach (var ch in chars)
            {
                columnCounter.Add(ch);

                var sourceChar = ch;
                var color = Color.Black;
                var backgroundColor = Color.Transparent;

#warning test
                //if (!CharUtil.CanDraw(ch) && ch != CharUtil.Tab)
                //{
                //    // バイナリファイルを開いた場合でも、それとなく表示できるようにしておく
                //    ch = '?';
                //}

                if (sourceChar == CharUtil.Tab && isTabProcess)
                {
                    sourceChar = CharUtil.Space;

                    for (var i = 0; i < columnCounter.LastCharCount; i++)
                    {
                        DebugUtil.DebugCode(() =>
                        {
                            sourceChar = i == 0 ? '>' : '.';
                            color = Color.DarkCyan;
                            backgroundColor = Color.FromRgb(0x00aaaa);
                        });

                        var width = _textView.AsciiFont.MeasureChar(sourceChar).Width;
                        yield return chItem.Init(
                            lineIndex, lineHeight,
                            sourceChar, x, index, originalIndex, 1,
                            color, backgroundColor, _textView.AsciiFont, width, _textView._settings.lineHeightAdjust / 2.0);
                        x += width;
                        index++;
                    }
                }
                else
                {
                    DebugUtil.DebugCode(() =>
                    {
                        if (sourceChar == CharUtil.FullWidthSpace)
                        {
                            sourceChar = '□';
                            color = Color.DarkCyan;
                        }
                        if (originalIndex >= lineLength)
                        {
                            color = Color.DarkCyan;
                        }
                    });

                    var tempChar = (sourceChar == CharUtil.Tab) ? CharUtil.Space : sourceChar;
                    var font = CharUtil.IsAscii(tempChar) ? _textView.AsciiFont : _textView.JpFont;
                    var width = font.MeasureChar(tempChar).Width * columnCounter.LastCharCount;

                    yield return chItem.Init(
                        lineIndex, lineHeight,
                        sourceChar, x, index, originalIndex, columnCounter.LastColumnCount,
                        color, backgroundColor, font, width, _textView._settings.lineHeightAdjust / 2.0);
                    x += width;
                    index++;
                }

                originalIndex++;
            }
        }
    }
}
