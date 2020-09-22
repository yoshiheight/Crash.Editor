using System.Collections.Generic;
using System.Linq;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI;
using Crash.Core.UI.Controls.ScrollBars;
using Crash.Core.UI.Controls.ScrollBars.Markers;

namespace Crash.Editor.Engine.View.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SolidRectMarker : IMarker
    {
        private readonly List<int> _markList = new List<int>();

        private readonly List<Rect2D> _rectList = new List<Rect2D>();

        private readonly IRectCalc _rectCalc;

        private readonly Color _color;

        public SolidRectMarker(IRectCalc rectCalc, Color color)
        {
            _rectCalc = rectCalc;
            _color = color;
        }

        public void AddMark(int value)
        {
            _markList.Add(value);
        }

        public void Clear()
        {
            _markList.Clear();
        }

        public void Draw(ScrollBar scrollBar, Renderer renderer)
        {
            if (scrollBar.Body.ClientRect.IsInvalid)
            {
                return;
            }

            foreach (var rect in _markList
                .Where(mark => scrollBar.IsInRangeValue(mark))
                .Select(mark => CalcRect(scrollBar, mark, _rectCalc)))
            {
                _rectList.UnionLastContinuousOrAdd(rect);
            }

            renderer.SetColor(_color);
            foreach (var rect in _rectList)
            {
                renderer.FillRect(rect);
            }
            _rectList.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        private static Rect2D CalcRect(ScrollBar scrollBar, int value, IRectCalc rectCalc)
        {
            var top = convertValueToPosition(value);
            var bottom = convertValueToPosition(value + 1);

            return rectCalc.CalcRect(top, bottom);

            // スクロール値を座標に変換する
            int convertValueToPosition(int value)
            {
                return scrollBar.IsThumbEnabled ?
                    scrollBar.Body.Thumb.ConvertValueToPosition(value)
                    : MathUtil.TruncateToInt32(scrollBar.Body.BodyPixels / (double)scrollBar.ViewportLength * value);
            }
        }
    }

    public interface IRectCalc
    {
        Rect2D CalcRect(int top, int bottom);
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class SolidRectCenterMarkInfo : IRectCalc
    {
        public Rect2D CalcRect(int top, int bottom)
        {
            var height = MathUtil.ClampMin(bottom - top, 2);
            return new Rect2D(7, top, 5, height);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class SolidRectLeftMarkInfo : IRectCalc
    {
        public Rect2D CalcRect(int top, int bottom)
        {
            var height = MathUtil.ClampMin(bottom - top, 2);
            return new Rect2D(0, top, 5, height);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class SolidRectRightMarkInfo : IRectCalc
    {
        public Rect2D CalcRect(int top, int bottom)
        {
            var height = MathUtil.ClampMin(bottom - top, 2);
            return new Rect2D(14, top, 5, height);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class SolidRectStretchMarkInfo : IRectCalc
    {
        public Rect2D CalcRect(int top, int bottom)
        {
            return new Rect2D(0, top, ScrollBar.Thickness, 2);
        }
    }
}
