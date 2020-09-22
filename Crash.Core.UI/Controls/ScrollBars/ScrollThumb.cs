using Crash.Core.Drawing;
using Crash.Core.UI.Common;

namespace Crash.Core.UI.Controls.ScrollBars
{
    /// <summary>
    /// スクロールつまみ。
    /// </summary>
    public sealed class ScrollThumb : UIElement
    {
        /// <summary>つまみの最小サイズ。</summary>
        private static readonly int MinLength = 15;

        /// <summary>スクロールバー。</summary>
        [Aggregation]
        private ScrollBar _scrollBar = null!;

        /// <summary>ドラッグ開始時のスクロール位置。</summary>
        private int? _dragStartValue = null;

        /// <summary>水平スクロールかどうか。</summary>
        private bool IsHorizontal => Direction == ScrollDirection.Horizontal;

        /// <summary>ドラッグによる移動ピクセル数。</summary>
        private int DragMoveLength => IsHorizontal ? SharedInfo.DragMoveOffset.Width : SharedInfo.DragMoveOffset.Height;

        /// <summary>スクロール可能ピクセル数。</summary>
        private int ScrollableLength => _scrollBar.Body.BodyPixels - ThumbLength;

        /// <summary>つまみの長さ。</summary>
        private int ThumbLength => MathUtil.ClampMin(
            MathUtil.TruncateToInt32(_scrollBar.Body.BodyPixels * (_scrollBar.ViewportLength / (double)_scrollBar.ContentLength)),
            MinLength);

        /// <summary>スクロール方向。</summary>
        public ScrollDirection Direction { get; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ScrollThumb(ScrollDirection direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewInitialize()
        {
            _scrollBar = Tnc.GetParent<ScrollBar>();
        }

        /// <summary>
        /// マウス左ボタンダウン。
        /// </summary>
        protected override void OnTargetMouseLeftButtonDown(MouseState _)
        {
            _dragStartValue = _scrollBar.Value;
            _scrollBar.RequestRender();
        }

        /// <summary>
        /// マウス左ボタンアップ。
        /// </summary>
        protected override void OnTargetMouseLeftButtonUp()
        {
            _dragStartValue = null;
            _scrollBar.UpdateLayout();
        }

        /// <summary>
        /// マウス左ボタンドラッグ。
        /// </summary>
        protected override void OnTargetMouseLeftButtonDrag(MouseState _)
        {
            var position = ConvertValueToPosition(_dragStartValue!.Value) + DragMoveLength;
            _scrollBar.Value = convertPositionToValue(position);
            _scrollBar.UpdateLayout();

            // つまみの座標をスクロール値に変換する（座標について、つまみの最小サイズを考慮）
            int convertPositionToValue(int position)
            {
                return MathUtil.RoundAwayFromZeroToInt32(_scrollBar.MaximumByScroll * (position / (double)ScrollableLength));
            }
        }

        /// <summary>
        /// スクロール値をつまみの座標に変換する（座標について、つまみの最小サイズを考慮）。
        /// </summary>
        public int ConvertValueToPosition(int value)
        {
            return MathUtil.TruncateToInt32(ScrollableLength / (double)_scrollBar.MaximumByScroll * value);
        }

        /// <summary>
        /// レイアウト調整。
        /// </summary>
        protected sealed override Rect2D ArrangeElement()
        {
            if (!_scrollBar.IsThumbEnabled)
            {
                return Rect2D.Empty;
            }

            int position;
            if (IsDragging)
            {
                // スクロール値１に対するつまみ移動のピクセル数が大きい場合でも、ピクセル単位でつまみを動かす為の処理
                position = ConvertValueToPosition(_dragStartValue!.Value) + DragMoveLength;
                position = MathUtil.Clamp(position, 0, ConvertValueToPosition(_scrollBar.MaximumByScroll));
            }
            else
            {
                position = ConvertValueToPosition(_scrollBar.Value);
            }

            return IsHorizontal ?
                new Rect2D(position, 0, ThumbLength, ScrollBar.Thickness)
                : new Rect2D(0, position, ScrollBar.Thickness, ThumbLength);
        }

        /// <summary>
        /// 描画。
        /// </summary>
        protected override void OnPreviewRender(Renderer renderer)
        {
            var color = IsDragging ? SharedInfo.Settings.ui_colors.scrollThumb_active : SharedInfo.Settings.ui_colors.scrollThumb;
            renderer.SetColor(color);

            var rect = IsHorizontal ? ClientRect.Inflate(0, -2) : ClientRect.Inflate(-2, 0);
            renderer.FillRect(rect);
        }
    }
}
