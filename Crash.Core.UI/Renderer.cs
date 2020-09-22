using System;
using System.Collections.Generic;
using System.Linq;
using Crash.Core.Drawing;
using Crash.Core.UI.UIContext;

namespace Crash.Core.UI
{
    /// <summary>
    /// レンダラー。
    /// </summary>
    public sealed class Renderer
    {
        /// <summary>レンダリングコンテキスト。</summary>
        [Aggregation]
        private readonly IOffscreen _offscreen;

        /// <summary></summary>
        [Aggregation]
        private readonly IReadOnlyRegion2D _clipRegion;

        /// <summary>現在のUIエレメントの実体の矩形。</summary>
        private readonly Rect2D _viewport;

        /// <summary>現在のUIエレメントの描画ビューポートの原点。</summary>
        private Point2D _contentOrigin = Point2D.Origin;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Renderer(IOffscreen offscreen, Rect2D viewport, IReadOnlyRegion2D clipRegion)
        {
            _offscreen = offscreen;
            _clipRegion = clipRegion;
            _viewport = viewport;
        }

        /// <summary>
        /// 
        /// </summary>
        public Renderer CreateRenderer(IOffscreen offscreen)
        {
            return new Renderer(offscreen, new Rect2D(Point2D.Origin, _viewport.Size), _clipRegion);
        }

        /// <summary>
        /// クリッピング領域
        /// </summary>
        public IReadOnlyRegion2D GetClipRegion()
        {
            var maskRect = new Rect2D(Point2D.Origin, _viewport.Size)
                .Offset(_contentOrigin.ToSize());
            return _clipRegion.Mask(maskRect);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetContentOrigin(Point2D origin)
        {
            _contentOrigin = origin;
        }

        /// <summary>
        /// トップレベルエレメントの座標系に変換する。
        /// </summary>
        private Point2D ToScreenCoord(Point2D pt)
        {
            return pt
                .Offset(_contentOrigin.InvertSign().ToSize())
                .Offset(_viewport.Location.ToSize());
        }

        /// <summary>
        /// トップレベルエレメントの座標系に変換する。
        /// </summary>
        private Rect2D ToScreenCoord(Rect2D rect)
        {
            return rect
                .Offset(_contentOrigin.InvertSign().ToSize())
                .Offset(_viewport.Location.ToSize());
        }

        /// <summary>
        /// 
        /// </summary>
        private IReadOnlyRegion2D ToScreenCoord(IReadOnlyRegion2D region)
        {
            return region
                .Offset(_contentOrigin.InvertSign().ToSize())
                .Offset(_viewport.Location.ToSize());
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetColor(Color color)
        {
            _offscreen.SetColor(color);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetFont(IFont font)
        {
            _offscreen.SetFont(font);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDisposable Save()
        {
            return _offscreen.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clip()
        {
            _offscreen.Clip(ToScreenCoord(GetClipRegion()));
        }

        /// <summary>
        /// 
        /// </summary>
        public void DrawString(ReadOnlySpan<char> text, Point2D pt)
        {
            if (text.IsEmpty || text.IsWhiteSpace()) return;

            _offscreen.DrawString(text, ToScreenCoord(pt));
        }

        /// <summary>
        /// 
        /// </summary>
        public void FillPolygon(IEnumerable<Point2D> points)
        {
            _offscreen.FillPolygon(points.Select(pt => ToScreenCoord(pt)));
        }

        /// <summary>
        /// 
        /// </summary>
        public void FillRect(Rect2D rect)
        {
            if (rect.IsInvalid) return;

            _offscreen.FillRect(ToScreenCoord(rect));
        }

        /// <summary>
        /// 
        /// </summary>
        public void DrawOffscreen(Renderer renderer, Point2D pt)
        {
            _offscreen.DrawOffscreen(renderer._offscreen, ToScreenCoord(pt));
        }
    }
}
