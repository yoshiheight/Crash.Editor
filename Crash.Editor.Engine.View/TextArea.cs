using System;
using System.Collections.Generic;
using System.Linq;
using Crash.Core;
using Crash.Core.Collections;
using Crash.Core.Drawing;
using Crash.Core.Text;
using Crash.Core.UI;
using Crash.Core.UI.Common;
using Crash.Core.UI.Controls.ScrollBars;
using Crash.Core.UI.Controls.ScrollBars.Markers;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.Model;
using Crash.Editor.Engine.View.Common;
using Crash.Editor.Engine.View.Common.Drawer;

namespace Crash.Editor.Engine.View
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TextArea : UIElement
    {
        /// <summary></summary>
        [Aggregation]
        private TextView _textView = null!;

        [DisposableField]
        private ScrollableOffscreen _scrollableOffscreen = null!;

        /// <summary></summary>
        private IReadOnlyTextDocument Doc => _textView.Doc;

        /// <summary></summary>
        [DisposableField]
        private MouseClickCounter _mouseClickCounter = null!;

        /// <summary></summary>
        [field: DisposableField]
        public Caret Caret { get; private set; } = null!;

        /// <summary></summary>
        private TextPos? _anchorPos;

        /// <summary></summary>
        private readonly SolidRectMarker _marker = new SolidRectMarker(new SolidRectStretchMarkInfo(), Color.Blue);

        /// <summary></summary>
        [Aggregation]
        private IInputMethod _inputMethod = null!;

        /// <summary></summary>
        private readonly PooledList<CharGroupDrawer> _charGroupDrawerList = new PooledList<CharGroupDrawer>();

        /// <summary></summary>
        private readonly PooledList<CharBackgroundGroupDrawer> _charBackgroundGroupDrawerList = new PooledList<CharBackgroundGroupDrawer>();

        /// <summary></summary>
        private readonly PooledList<SelectionGroupDrawer> _lineSelectionDrawerList = new PooledList<SelectionGroupDrawer>();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        internal TextArea()
        {
        }

        /// <summary></summary>
        public bool IsSelected => !SelectionRange.IsEmpty;

        /// <summary></summary>
        public TextRange SelectionRange => new TextRange(_anchorPos ?? Caret.Pos, Caret.Pos);

        /// <summary></summary>
        public IReadOnlyTextLine CurrentLine => Doc.Lines[Caret.Pos.LineIndex];

        /// <summary></summary>
        public TextPos CurrentLineTopPos => new TextPos(Caret.Pos.LineIndex, 0);

        /// <summary></summary>
        public TextPos CurrentLineEndPos => new TextPos(Caret.Pos.LineIndex, Doc.Lines[Caret.Pos.LineIndex].Length);

        public TextPos TopPos => TextPos.GetTopPos();

        /// <summary></summary>
        public TextPos EndPos => TextPos.GetEndPos(Doc);

        internal void RequestRenderByHScroll()
        {
            _scrollableOffscreen.RequestRenderByHScroll(_textView.HScrollBar.Value, _textView.HScrollStep);
        }

        internal void RequestRenderByVScroll()
        {
            _scrollableOffscreen.RequestRenderByVScroll(_textView.VScrollBar.Value, _textView.VScrollStep);
        }

        private const int VScrollMargin = 1;

        const int HScrollMargin = 2;

        public static readonly int HScrollMax = 1000;

        public int VContentLength => Doc.Lines.Count + VScrollMargin;

        public int HContentLength => HScrollMax + HScrollMargin;

        public const int ScrollingMargin = 1;

        public override Rect2D ContentRect => new Rect2D(
            0,
            0,
            HContentLength * _textView.HScrollStep,
            (VContentLength + ScrollingMargin) * _textView.VScrollStep).Union(ClientRect);

        /// <summary>
        /// 現在位置の単語を選択する。
        /// </summary>
        public void SelectWord()
        {
            if (CurrentLine.IsEmpty)
            {
                return;
            }

            var text = CurrentLine.Text;
            var charIndex = (Caret.Pos.CharIndex < text.Length) ? Caret.Pos.CharIndex : Caret.Pos.CharIndex - 1;
            var currentChar = text[charIndex];

            var prevCharIndex = charIndex;
            while (0 < prevCharIndex && CharUtil.EqualsWordGroup(currentChar, text[prevCharIndex - 1]))
            {
                prevCharIndex--;
            }

            var nextCharIndex = charIndex + 1;
            while (nextCharIndex < text.Length && CharUtil.EqualsWordGroup(currentChar, text[nextCharIndex]))
            {
                nextCharIndex++;
            }

            ClearSelect();
            _anchorPos = new TextPos(Caret.Pos.LineIndex, prevCharIndex);
            SetCaretPos(new TextPos(Caret.Pos.LineIndex, nextCharIndex), true, true);
        }

        /// <summary>
        /// 行選択。
        /// </summary>
        public void SelectLine()
        {
            ClearSelect();
            _anchorPos = CurrentLineTopPos;
            SetCaretPos(CurrentLineEndPos.GetNextPos(Doc), true, true, false);
        }

        /// <summary>
        /// 全選択。
        /// </summary>
        public void SelectAll()
        {
            _anchorPos = TopPos;
            SetCaretPos(EndPos, true, true, false);
            RequestRender();
        }

        /// <summary>
        /// 選択状態をクリアする。
        /// </summary>
        public void ClearSelect()
        {
            if (IsSelected)
            {
                RequestRenderLinesByRange(this, SelectionRange);
            }
            _anchorPos = null;
        }

        /// <summary>
        /// 上へ移動。
        /// </summary>
        public void MoveUp(bool isSelect)
        {
            MoveUpDown(-1, isSelect);
        }

        /// <summary>
        /// 下へ移動。
        /// </summary>
        public void MoveDown(bool isSelect)
        {
            MoveUpDown(1, isSelect);
        }

        /// <summary>
        /// 上 or 下へ移動。
        /// </summary>
        private void MoveUpDown(int offset, bool isSelect)
        {
            var lineIndex = MathUtil.Clamp(Caret.Pos.LineIndex + offset, 0, EndPos.LineIndex);
            var charIndex = _textView.DrawingMetricsMeasurer.CalcCharIndex(lineIndex, Caret.BaseX);

            SetCaretPos(new TextPos(lineIndex, charIndex), isSelect, false);
        }

        /// <summary>
        /// 左へ移動。
        /// </summary>
        /// <param name="isSelect"></param>
        public void MoveLeft(bool isSelect)
        {
            var pos = (IsSelected && !isSelect) ? SelectionRange.Start : Caret.Pos.GetPrevPos(Doc);
            SetCaretPos(pos, isSelect, true);
        }

        /// <summary>
        /// 右へ移動。
        /// </summary>
        public void MoveRight(bool isSelect)
        {
            var pos = (IsSelected && !isSelect) ? SelectionRange.End : Caret.Pos.GetNextPos(Doc);
            SetCaretPos(pos, isSelect, true);
        }

        /// <summary>
        /// 行頭へ移動。
        /// </summary>
        public void MoveLineTop(bool isSelect)
        {
            SetCaretPos(CurrentLineTopPos, isSelect, true);
        }

        /// <summary>
        /// 行末へ移動。
        /// </summary>
        public void MoveLineEnd(bool isSelect)
        {
            SetCaretPos(CurrentLineEndPos, isSelect, true);
        }

        /// <summary>
        /// 指定の位置へ移動。
        /// </summary>
        public void MovePos(TextPos pos, bool isSelect)
        {
            SetCaretPos(pos, isSelect, true);
        }

        /// <summary>
        /// 前の単語へ移動。
        /// </summary>
        public void MovePrevWord(bool isSelect)
        {
            if (Caret.Pos == CurrentLineTopPos)
            {
                MoveLeft(isSelect);
                return;
            }

            var text = CurrentLine.Text;
            var charIndex = Caret.Pos.CharIndex - 1;
            while (0 < charIndex && Char.IsWhiteSpace(text[charIndex]))
            {
                charIndex--;
            }
            while (0 < charIndex && CharUtil.EqualsWordGroup(text[charIndex], text[charIndex - 1]))
            {
                charIndex--;
            }

            SetCaretPos(new TextPos(Caret.Pos.LineIndex, charIndex), isSelect, true);
        }

        /// <summary>
        /// 次の単語へ移動。
        /// </summary>
        public void MoveNextWord(bool isSelect)
        {
            if (Caret.Pos == CurrentLineEndPos)
            {
                MoveRight(isSelect);
                return;
            }

            var text = CurrentLine.Text;
            var charIndex = Caret.Pos.CharIndex + 1;
            while (charIndex < text.Length && CharUtil.EqualsWordGroup(text[charIndex - 1], text[charIndex]))
            {
                charIndex++;
            }
            while (charIndex < text.Length && Char.IsWhiteSpace(text[charIndex]))
            {
                charIndex++;
            }

            SetCaretPos(new TextPos(Caret.Pos.LineIndex, charIndex), isSelect, true);
        }

        /// <summary>
        /// ページアップ。
        /// </summary>
        public void MovePageUp(bool isSelect)
        {
            _textView.ScrollPageUp();

            MovePageUpDown(-_textView.VScrollBar.ViewportLength, isSelect);
        }

        /// <summary>
        /// ページダウン。
        /// </summary>
        public void MovePageDown(bool isSelect)
        {
            _textView.ScrollPageDown();

            MovePageUpDown(_textView.VScrollBar.ViewportLength, isSelect);
        }

        /// <summary>
        /// ページアップ or ページダウン。
        /// </summary>
        private void MovePageUpDown(int offset, bool isSelect)
        {
            var lineIndex = MathUtil.Clamp(Caret.Pos.LineIndex + offset, 0, EndPos.LineIndex);
            var charIndex = _textView.DrawingMetricsMeasurer.CalcCharIndex(lineIndex, Caret.BaseX);

            SetCaretPos(new TextPos(lineIndex, charIndex), isSelect, false);
        }

        /// <summary>
        /// 先頭へ移動。
        /// </summary>
        public void MoveTop(bool isSelect)
        {
            SetCaretPos(TopPos, isSelect, true);
        }

        /// <summary>
        /// 末尾へ移動。
        /// </summary>
        public void MoveEnd(bool isSelect)
        {
            SetCaretPos(EndPos, isSelect, true);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewInitialize()
        {
            _textView = Tnc.GetParent<TextView>();
            _mouseClickCounter = new MouseClickCounter(4);

            _textView.VScrollBar.SetMarks(new List<IMarker> { _marker });

            Caret = new Caret(_textView);
            Caret.BlinkChanged += () => RequestRenderCurrentLine();

            _textView.FontChanged += () =>
            {
                var jsFontInfo = _textView._settings.fonts.jpFont;
                _inputMethod.SetFont(jsFontInfo.name, jsFontInfo.height * _textView.ZoomRate, _textView.GetLineHeight());
                Caret.Update();
            };

            Doc.DocModified += OnDocumentModified;
        }

        protected override void OnPreviewCreated()
        {
            _scrollableOffscreen = new ScrollableOffscreen(this);

            _inputMethod = CanvasContext.GetInputMethod();
            var jsFontInfo = _textView._settings.fonts.jpFont;
            _inputMethod.SetFont(jsFontInfo.name, jsFontInfo.height * _textView.ZoomRate, _textView.GetLineHeight());
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewGotFocus()
        {
            Caret.UpdateOnFocusChanged();
            RequestRender();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewLostFocus()
        {
            Caret.UpdateOnFocusChanged();
            RequestRender();
        }

        protected override void OnPreviewMouseMove(MouseState mei, ref bool handled)
        {
            CanvasContext.SetCursor(Cursor.Text);
            handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDocumentModified(ModifyEventArgs e)
        {
            (TextPos, bool, bool) setCaretPosParams;
            switch (e.ModifyStatus, e.ModifyDetail)
            {
                case (ModifyStatus.Modify, ModifyDetail.IndentOrUnindent):
                    _anchorPos = e.Range.OriginalStart;
                    setCaretPosParams = (e.Range.OriginalEnd, true, true);
                    break;
                case (ModifyStatus.Modify, _):
                    setCaretPosParams = (e.Range.End, false, true);
                    break;
                case (ModifyStatus.Undo, ModifyDetail.RemoveOne):
                    setCaretPosParams = (e.Range.OriginalEnd, false, true);
                    break;
                case (ModifyStatus.Undo, _):
                    _anchorPos = e.Range.OriginalStart;
                    setCaretPosParams = (e.Range.OriginalEnd, true, true);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            SetCaretPos(setCaretPosParams.Item1, setCaretPosParams.Item2, setCaretPosParams.Item3);

            var range = e.Range;
            if (EndPos.LineIndex != e.OldEndPos.LineIndex)
            {
                range = new TextRange(e.Range.Start, MathUtil.Max(EndPos, e.OldEndPos));
            }
            RequestRenderLinesByRange(this, range);
            RequestRenderLinesByRange(_textView.VRuler, range);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Rect2D ArrangeElement()
        {
            var vRulerWidth = _textView.VRuler.CalcWidth();
            var width = Tnc.GetParent().ClientSize.Width - vRulerWidth - ScrollBar.Thickness;
            var height = Tnc.GetParent().ClientSize.Height - ScrollBar.Thickness;
            return new Rect2D(vRulerWidth, 0, width, height);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnTargetMouseLeftButtonDown(MouseState mouseState)
        {
            _mouseClickCounter.Add(mouseState.Location);
            switch (_mouseClickCounter.ClickCount, mouseState.IsModifierKey)
            {
                case (1, _): MovePosByLocation(ToLocal(mouseState.Location), mouseState.IsShift); break;
                case (2, false): SelectWord(); break;
                case (3, false): SelectLine(); break;
                case (4, false): SelectAll(); break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnTargetMouseLeftButtonDrag(MouseState mei)
        {
            // ドラッグ中
            MovePosByLocation(ToLocal(mei.Location), true);
        }

        /// <summary>
        /// 再描画要求。レンジが空でも要求する。
        /// </summary>
        private void RequestRenderLinesByRange(UIElement element, TextRange range)
        {
            var y = range.Start.LineIndex * _textView.GetLineHeight();
            var height = range.LineCount * _textView.GetLineHeight();

            // 横幅はテキストエリアの幅とする
            element.RequestRenderLocal(new Rect2D(0, y, element.ContentRect.Width, height));
        }

        /// <summary>
        /// 
        /// </summary>
        internal void RequestRenderCurrentLine()
        {
            RequestRenderLinesByRange(this, new TextRange(Caret.Pos, Caret.Pos));
        }

        /// <summary>
        /// 
        /// </summary>
        private void MovePosByLocation(Point2D location, bool isSelect)
        {
            var lineIndex = location.Y / _textView.GetLineHeight() + _textView.VScrollBar.Value;
            lineIndex = MathUtil.Clamp(lineIndex, 0, EndPos.LineIndex);
            var charIndex = _textView.DrawingMetricsMeasurer.CalcCharIndex(lineIndex, location.X + _textView.GetHScrolledWidth());
            var textPos = new TextPos(lineIndex, charIndex);

            MovePos(textPos, isSelect);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetCaretPos(TextPos newPos, bool isSelect, bool isResetBaseColumn, bool isScroll = true)
        {
            var oldCaretPos = Caret.Pos;
            var oldSelectionRange = SelectionRange;

            Caret.Move(newPos, isResetBaseColumn);
            _marker.Clear();
            _marker.AddMark(Caret.Pos.LineIndex);
            _textView.VScrollBar.RequestRender();

            if (isScroll)
            {
                _textView.ScrollCaret();
            }

            if (isSelect)
            {
                _anchorPos ??= oldCaretPos;
                RequestRenderLinesByRange(this, new TextRange(oldCaretPos, Caret.Pos));
            }
            else
            {
                _anchorPos = null;
                if (!oldSelectionRange.IsEmpty)
                {
                    RequestRenderLinesByRange(this, oldSelectionRange);
                }
                else
                {
                    RequestRenderLinesByRange(this, new TextRange(oldCaretPos, oldCaretPos));
                }
                RequestRenderCurrentLine();
            }
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
            _inputMethod.SetArea(AbsoluteRect, Caret.ScrolledAbsLocation);

            var offscreenRenderer = _scrollableOffscreen.PrepareRender(renderer);
            Render(offscreenRenderer);
            renderer.DrawOffscreen(offscreenRenderer, Point2D.Origin);
        }

        /// <summary>
        /// 描画。
        /// </summary>
        private void Render(Renderer renderer)
        {
            renderer.SetContentOrigin(_textView.GetScrolledSize().ToPoint());
            using var __ = renderer.Save();
            renderer.Clip();

            var clipRegion = renderer.GetClipRegion();

            renderer.SetColor(Color.White);
            renderer.FillRect(clipRegion.Bounds);

            if (!IsSelected)
            {
                var currentLineRect = new Rect2D(_textView.GetHScrolledWidth(), Caret.Location.Y, ClientSize.Width, _textView.GetLineHeight());
                renderer.SetColor(Color.FromRgb(0xEAEAF2));
                renderer.FillRect(currentLineRect);
                renderer.SetColor(Color.White);
                renderer.FillRect(currentLineRect.Inflate(-2, -2));
            }

            foreach (var metric in _textView.DrawingMetricsMeasurer.EnumerateCharMetrics(clipRegion))
            {
                // 文字
                if (_charGroupDrawerList.LastOrNull()?.TryAdd(metric) != true)
                {
                    _charGroupDrawerList.Add().Init(metric);
                }

                // 文字の背景色
                if (!metric.BackgroundColor.IsTransparent
                    && _charBackgroundGroupDrawerList.LastOrNull()?.TryAdd(metric) != true)
                {
                    _charBackgroundGroupDrawerList.Add().Init(metric);
                }

                // 選択範囲
                if (SelectionRange.Contains(metric.CharRange)
                    && _lineSelectionDrawerList.LastOrNull()?.TryAdd(metric) != true)
                {
                    _lineSelectionDrawerList.Add().Init(metric);
                }
            }

            foreach (var drawer in _charBackgroundGroupDrawerList.GetElementsAndClear())
            {
                drawer.Draw(renderer);
            }

            foreach (var drawer in _lineSelectionDrawerList.GetElementsAndClear())
            {
                drawer.Draw(renderer, SharedInfo.HasFocus);
            }

            foreach (var drawer in _charGroupDrawerList.GetElementsAndClear()
                .GroupBy(d => d.Font)
                .SelectMany(g => g
                    .GroupBy(d => d.Color)
                    .SelectMany(g => g)))
            {
                drawer.Draw(renderer);
            }

            Caret.Draw(renderer);
        }
    }
}
