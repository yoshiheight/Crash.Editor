using System;
using System.Collections.Generic;
using Crash.Core.Drawing;

namespace Crash.Core.UI.UIContext
{
    /// <summary>
    /// 
    /// </summary>
    public interface IOffscreen : IDisposable
    {
        void SetColor(Color color);

        void SetFont(IFont font);

        void Resize(Size2D size);

        /// <summary>
        /// 現在の状態を保存し、IDisposableによって復元する。
        /// 対象となる状態は、クリップ領域は必須、それ以外は任意。
        /// </summary>
        IDisposable Save();

        void Clip(IReadOnlyRegion2D region);

        void DrawString(ReadOnlySpan<char> text, Point2D pt);

        void FillPolygon(IEnumerable<Point2D> points);

        void FillRect(Rect2D rect);

        void DrawOffscreen(IOffscreen offscreen, Point2D pt);
    }
}
