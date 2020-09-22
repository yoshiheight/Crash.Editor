using Crash.Core.Drawing;

namespace Crash.Core.UI.Common
{
    /// <summary>
    /// マウスクリックカウンター。
    /// </summary>
    public sealed class MouseClickCounter : IAutoFieldDisposable
    {
        [DisposableField]
        private readonly UITimer _timer;

        private readonly int _clickMax;

        private Point2D _location;

        public int ClickCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MouseClickCounter(int clickMax)
        {
            _timer = new UITimer(500, false, () =>
            {
                _location = Point2D.Origin;
                ClickCount = 0;
            });

            _clickMax = clickMax;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Add(Point2D location)
        {
            if (ClickCount > 0 && location != _location)
            {
                ClickCount = 0;
            }

            _location = location;
            ClickCount++;
            if (ClickCount > _clickMax)
            {
                ClickCount = 1;
            }
            _timer.Restart();
        }
    }
}
