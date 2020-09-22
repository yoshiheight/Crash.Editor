using System;
using Crash.Core.Drawing;
using Crash.Core.UI.UIContext;

namespace Crash.Core.UI.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ScrollableOffscreen : IAutoFieldDisposable
    {
        private readonly UIElement _element;

        [DisposableField]
        private readonly IOffscreen _primaryOffscreen;

        [DisposableField]
        private readonly IOffscreen _secondaryOffscreen;

        private (int oldValue, int offsetTotal) _hscrollState;

        private (int oldValue, int offsetTotal) _vscrollState;

        private Rect2D ContentRect => _element.ContentRect;

        private Size2D Size => _element.ClientSize;

        /// <summary>
        /// 
        /// </summary>
        public ScrollableOffscreen(UIElement element)
        {
            _element = element;

            _primaryOffscreen = _element.CanvasContext.CreateOffscreen(Size);
            _secondaryOffscreen = _element.CanvasContext.CreateOffscreen(Size);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resize()
        {
            _primaryOffscreen.Resize(Size);
            _secondaryOffscreen.Resize(Size);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestRenderByHScroll(int value, int scrollStep)
        {
            var offset = (value - _hscrollState.oldValue) * scrollStep;
            Verifier.Verify<ArgumentException>(offset != 0);

            var rect = new Rect2D(0, 0, Math.Abs(offset), ContentRect.Height)
                .OffsetX((offset > 0) ? Size.Width - Math.Abs(offset) : 0)
                .OffsetX(value * scrollStep);

            _element.RequestRenderLocal(rect);

            _hscrollState.oldValue = value;
            _hscrollState.offsetTotal += offset;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestRenderByVScroll(int value, int scrollStep)
        {
            var offset = (value - _vscrollState.oldValue) * scrollStep;
            Verifier.Verify<ArgumentException>(offset != 0);

            var rect = new Rect2D(0, 0, ContentRect.Width, Math.Abs(offset))
                .OffsetY((offset > 0) ? Size.Height - Math.Abs(offset) : 0)
                .OffsetY(value * scrollStep);

            _element.RequestRenderLocal(rect);

            _vscrollState.oldValue = value;
            _vscrollState.offsetTotal += offset;
        }

        /// <summary>
        /// 
        /// </summary>
        public Renderer PrepareRender(Renderer renderer)
        {
            // memo 再描画要求を出してから実際に描画イベントが発生するまでの間にスクロールイベントが複数回発生する場合がある

            var offset = new Size2D(_hscrollState.offsetTotal, _vscrollState.offsetTotal);
            if (offset.Width != 0 || offset.Height != 0)
            {
                if (Math.Abs(offset.Width) < Size.Width && Math.Abs(offset.Height) < Size.Height)
                {
                    _secondaryOffscreen.DrawOffscreen(_primaryOffscreen, offset.InvertSign().ToPoint());
                    _primaryOffscreen.DrawOffscreen(_secondaryOffscreen, Point2D.Origin);
                }

                _hscrollState.offsetTotal = 0;
                _vscrollState.offsetTotal = 0;
            }

            return renderer.CreateRenderer(_primaryOffscreen);
        }
    }
}
