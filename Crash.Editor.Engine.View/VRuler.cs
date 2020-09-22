using System;
using System.Collections.Generic;
using Crash.Core;
using Crash.Core.Collections;
using Crash.Core.Drawing;
using Crash.Core.Text;
using Crash.Core.UI;
using Crash.Core.UI.Common;
using Crash.Core.UI.Controls.ScrollBars;
using Crash.Editor.Engine.Model;

namespace Crash.Editor.Engine.View
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class VRuler : UIElement
    {
        /// <summary></summary>
        [Aggregation]
        private TextView _textView = null!;

        /// <summary></summary>
        [DisposableField]
        private ScrollableOffscreen _scrollableOffscreen = null!;

        /// <summary></summary>
        public override Rect2D ContentRect => new Rect2D(
            0,
            0,
            ClientSize.Width,
            (_textView.TextArea.VContentLength + TextArea.ScrollingMargin) * _textView.VScrollStep).Union(ClientRect);

        /// <summary></summary>
        private readonly Dictionary<Color, List<Rect2D>> _markListMap = new Dictionary<Color, List<Rect2D>>();

        /// <summary>表示桁数。</summary>
        private int CalcDisplayDigits() => MathUtil.ClampMin(MathUtil.CalcDigits(_textView.Doc.Lines.Count), 3);

        /// <summary></summary>
        public int CalcWidth() => CharSize.RoundWidth(
            _textView.AsciiFont.MeasureChar('9').Width * CalcDisplayDigits()) + 45;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        internal VRuler()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        internal void RequestRenderByVScroll()
        {
            _scrollableOffscreen.RequestRenderByVScroll(_textView.VScrollBar.Value, _textView.VScrollStep);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewInitialize()
        {
            _textView = Tnc.GetParent<TextView>();

            var oldDisplayDigits = -1;
            _textView.Doc.DocModified += _ =>
            {
                var displayDigits = CalcDisplayDigits();
                if (oldDisplayDigits != displayDigits)
                {
                    oldDisplayDigits = displayDigits;
                    _textView.UpdateLayout();
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewCreated()
        {
            _scrollableOffscreen = new ScrollableOffscreen(this);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewResize()
        {
            _scrollableOffscreen.Resize();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewRender(Renderer renderer)
        {
            var offscreenRenderer = _scrollableOffscreen.PrepareRender(renderer);
            Render(offscreenRenderer);
            renderer.DrawOffscreen(offscreenRenderer, Point2D.Origin);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Render(Renderer renderer)
        {
            renderer.SetContentOrigin(new Point2D(0, _textView.GetScrolledSize().Height));
            using var __ = renderer.Save();
            renderer.Clip();

            var clipRegion = renderer.GetClipRegion();

            renderer.SetColor(Color.White);
            renderer.FillRect(clipRegion.Bounds);

            var clientRect = ClientRect;
            var lineHeight = _textView.GetLineHeight();
            var displayDigits = CalcDisplayDigits();

            renderer.SetColor(Color.FromRgb(0x237893));
            renderer.SetFont(_textView.AsciiFont);

            Span<char> text = stackalloc char[displayDigits];
            foreach (var metric in _textView.DrawingMetricsMeasurer.EnumerateLineMetrics(clipRegion))
            {
#warning test
                // 行番号
                // TryFormatでカスタム書式指定が機能しないので（.NETのバグ？）、自前で先頭空白埋めする
                text.Fill(CharUtil.Space);
                var lineNumber = metric.Index + 1;
                lineNumber.TryFormat(text.Slice(displayDigits - MathUtil.CalcDigits(lineNumber)), out _);
                renderer.DrawString(text, new Point2D(20, metric.Y));

                // 境界線
                var lineRect = new Rect2D(clientRect.Right - 5, metric.Y, 1, lineHeight);
                _markListMap
                    .GetOrAdd(Color.Gray, _ => new List<Rect2D>())
                    .UnionLastContinuousOrAdd(lineRect);

                // 更新マーク
                if (GetMarkColor(_textView.Doc.Lines[metric.Index].ModifyStatus) is { } color)
                {
                    var markRect = new Rect2D(clientRect.Right - 15, metric.Y, 5, lineHeight);
                    _markListMap
                        .GetOrAdd(color, _ => new List<Rect2D>())
                        .UnionLastContinuousOrAdd(markRect);
                }
            }

            foreach (var (color, rectList) in _markListMap)
            {
                renderer.SetColor(color);
                foreach (var rect in rectList)
                {
                    renderer.FillRect(rect);
                }
                rectList.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Color? GetMarkColor(LineModifyStatus lineModifyStatus)
        {
            return lineModifyStatus switch
            {
                LineModifyStatus.Modified => Color.SandyBrown,
                LineModifyStatus.ModifySaved => Color.CadetBlue,
                LineModifyStatus.None => null,
                _ => throw new InvalidOperationException(),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Rect2D ArrangeElement()
        {
            var vRulerWidth = CalcWidth();
            return new Rect2D(0, 0, vRulerWidth, _textView.ClientSize.Height - ScrollBar.Thickness);
        }
    }
}
