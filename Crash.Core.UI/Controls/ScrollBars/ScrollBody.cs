using System;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;

namespace Crash.Core.UI.Controls.ScrollBars
{
    /// <summary>
    /// スクロールエリア基底クラス。
    /// </summary>
    public sealed class ScrollBody : UIElement
    {
        [DisposableField]
        private UITimer? _timer;

        public ScrollBar ScrollBar { get; private set; } = null!;

        public ScrollThumb Thumb { get; private set; } = null!;

        public int BodyPixels => Direction == ScrollDirection.Horizontal ? ClientSize.Width : ClientSize.Height;

        public ScrollDirection Direction { get; }

        public ScrollBody(ScrollDirection direction)
        {
            Direction = direction;

            Tnc.AddNode(new ScrollThumb(Direction));
        }

        protected override void OnPreviewInitialize()
        {
            ScrollBar = Tnc.GetParent<ScrollBar>();
            Thumb = Tnc.GetChild<ScrollThumb>();
        }

        protected override void OnRender(Renderer renderer)
        {
            if (ScrollBar.IsMarkerEnabled)
            {
                foreach (var mark in ScrollBar.Marks)
                {
                    mark.Draw(ScrollBar, renderer);
                }
            }
        }

        protected override void OnPreviewRender(Renderer renderer)
        {
            renderer.SetColor(SharedInfo.Settings.ui_colors.background);
            renderer.FillRect(ClientRect);
        }

        protected override void OnTargetMouseLeftButtonDown(MouseState mei)
        {
            if (!ScrollBar.IsThumbEnabled)
            {
                return;
            }

            var location = ToLocal(mei.Location);
            var firstDirection = calcDirection();

            updateScrollValue();

            _timer = new UITimer(300, false, () =>
            {
                _timer = new UITimer(33, true, () => updateScrollValue());
                _timer.Start();
            });
            _timer.Start();

            void updateScrollValue()
            {
                var direction = calcDirection();
                if (direction == firstDirection)
                {
                    ScrollBar.Value += ScrollBar.ViewportLength * direction;
                }
            }

            int calcDirection()
            {
                return (location.X < Thumb.RelativeRect.Left || location.Y < Thumb.RelativeRect.Top) ? -1
                    : (location.X >= Thumb.RelativeRect.Right || location.Y >= Thumb.RelativeRect.Bottom) ? 1
                    : 0;
            }
        }

        protected override void OnTargetMouseLeftButtonUp()
        {
            _timer?.Stop();
            _timer = null;
        }

        protected override Rect2D ArrangeElement()
        {
            switch (Direction)
            {
                case ScrollDirection.Vertical:
                    var height = Tnc.GetParent().ClientSize.Height - ScrollButton.Length * 2;
                    return new Rect2D(0, ScrollButton.Length, ScrollBar.Thickness, height);
                case ScrollDirection.Horizontal:
                    var width = Tnc.GetParent().ClientSize.Width - ScrollButton.Length * 2;
                    return new Rect2D(ScrollButton.Length, 0, width, ScrollBar.Thickness);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
