using System;
using System.Collections.Generic;
using System.Linq;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;

namespace Crash.Core.UI.Controls.ScrollBars
{
    /// <summary>
    /// スクロールボタン。
    /// </summary>
    public sealed class ScrollButton : UIElement
    {
        public static readonly int Length = 16;

        [DisposableField]
        private UITimer? _timer;

        public ScrollBar ScrollBar { get; private set; } = null!;

        public ScrollDirection Direction { get; }

        public bool IsPrevButton { get; }

        private Color ArrowColor => !ScrollBar.IsScrollEnabled ? SharedInfo.Settings.ui_colors.scrollArrow_disable
            : IsDragging ? SharedInfo.Settings.ui_colors.scrollArrow_active
            : SharedInfo.Settings.ui_colors.scrollArrow;

        public ScrollButton(ScrollDirection direction, bool isPrevButton)
        {
            Direction = direction;
            IsPrevButton = isPrevButton;
        }

        protected override void OnPreviewInitialize()
        {
            ScrollBar = Tnc.GetParent<ScrollBar>();
        }

        protected override void OnTargetMouseLeftButtonDown(MouseState mei)
        {
            RequestRender();

            updateScrollValue();

            _timer = new UITimer(300, false, () =>
            {
                _timer = new UITimer(33, true, () => updateScrollValue());
                _timer.Start();
            });
            _timer.Start();

            void updateScrollValue()
            {
                ScrollBar.Value += ScrollBar.ArrowStep * (IsPrevButton ? -1 : 1);
            }
        }

        protected override void OnTargetMouseLeftButtonUp()
        {
            RequestRender();

            _timer?.Stop();
            _timer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewRender(Renderer renderer)
        {
            switch (Direction)
            {
                case ScrollDirection.Vertical:
                    renderVertical();
                    break;
                case ScrollDirection.Horizontal:
                    renderHorizontal();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            void renderVertical()
            {
                var points = getPoints(ClientSize);
                points = IsPrevButton ? points : points.Select(pt => new Point2D(pt.X, ClientSize.Height - pt.Y));

                renderer.SetColor(SharedInfo.Settings.ui_colors.background);
                renderer.FillRect(ClientRect);

                renderer.SetColor(ArrowColor);
                renderer.FillPolygon(points);

                // 座標定義
                static IEnumerable<Point2D> getPoints(Size2D clientSize)
                {
                    var pt1 = new Point2D(clientSize.Width / 2, clientSize.Height / 3);
                    var height = clientSize.Height / 3;
                    var width = (int)(height * 1.7);
                    yield return pt1;
                    yield return new Point2D(pt1.X + width / 2, pt1.Y + height);
                    yield return new Point2D(pt1.X - width / 2, pt1.Y + height);
                }
            }

            void renderHorizontal()
            {
                var points = getPoints(ClientSize);
                points = IsPrevButton ? points : points.Select(pt => new Point2D(ClientSize.Width - pt.X, pt.Y));

                renderer.SetColor(SharedInfo.Settings.ui_colors.background);
                renderer.FillRect(ClientRect);

                renderer.SetColor(ArrowColor);
                renderer.FillPolygon(points);

                // 座標定義
                static IEnumerable<Point2D> getPoints(Size2D clientSize)
                {
                    var pt1 = new Point2D(clientSize.Width / 3, clientSize.Height / 2);
                    var width = clientSize.Width / 3;
                    var height = (int)(width * 1.7);
                    yield return pt1;
                    yield return new Point2D(pt1.X + width, pt1.Y - height / 2);
                    yield return new Point2D(pt1.X + width, pt1.Y + height / 2);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Rect2D ArrangeElement()
        {
            return (Direction, IsPrevButton) switch
            {
                (ScrollDirection.Vertical, true) => new Rect2D(0, 0, ScrollBar.Thickness, Length),
                (ScrollDirection.Vertical, false) => new Rect2D(0, Tnc.GetParent().ClientSize.Height - Length, ScrollBar.Thickness, Length),
                (ScrollDirection.Horizontal, true) => new Rect2D(0, 0, Length, ScrollBar.Thickness),
                (ScrollDirection.Horizontal, false) => new Rect2D(Tnc.GetParent().ClientSize.Width - Length, 0, Length, ScrollBar.Thickness),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
