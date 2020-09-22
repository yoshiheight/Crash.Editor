using System;
using System.Collections.Generic;
using System.Linq;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI.UIContext;

namespace Sample.WinForms.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Offscreen : IOffscreen, IAutoFieldDisposable
    {
        [field: DisposableField(Order = 1)]
        internal System.Drawing.Image Image { get; private set; } = null!;

        [field: DisposableField(Order = 0)]
        internal System.Drawing.Graphics Graphics { get; private set; } = null!;

        private System.Drawing.Color? _currentColor;

        [field: Aggregation]
        private System.Drawing.Font? _currentFont;

        /// <summary>
        /// 
        /// </summary>
        public Offscreen(Size2D size)
        {
            Recreate(size);
        }

        public void Resize(Size2D size)
        {
            Recreate(size);
        }

        private void Recreate(Size2D size)
        {
            IAutoFieldDisposable.DisposeFields(this);

            size = size.ClampMin(1, 1);
            Image = new System.Drawing.Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics = System.Drawing.Graphics.FromImage(Image);
            Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        }

        internal void Flush()
        {
            Graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
        }

        public void SetColor(Color color)
        {
            _currentColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public void SetFont(IFont font)
        {
            _currentFont = font.DownCast<Font>().Value;
        }

        public IDisposable Save()
        {
            var state = Graphics.Save();
            return Disposable.Create(() => Graphics.Restore(state));
        }

        public void Clip(IReadOnlyRegion2D region)
        {
            var clip = new System.Drawing.Region(System.Drawing.Rectangle.Empty);
            foreach (var rect in region.Rects)
            {
                clip.Union(new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            }
            Graphics.SetClip(clip, System.Drawing.Drawing2D.CombineMode.Replace);
        }

        public void DrawString(ReadOnlySpan<char> text, Point2D pt)
        {
            using (var brush = new System.Drawing.SolidBrush(_currentColor!.Value))
            {
                Graphics.DrawString(new string(text), _currentFont!, brush,
                    new System.Drawing.Point(pt.X, pt.Y), System.Drawing.StringFormat.GenericTypographic);
            }
        }

        public void FillPolygon(IEnumerable<Point2D> points)
        {
            using (var brush = new System.Drawing.SolidBrush(_currentColor!.Value))
            {
                var polygon = points
                    .Select(pt => new System.Drawing.Point(pt.X, pt.Y))
                    .ToArray();
                Graphics.FillPolygon(brush, polygon);
            }
        }

        public void FillRect(Rect2D rect)
        {
            using (var brush = new System.Drawing.SolidBrush(_currentColor!.Value))
            {
                var oldSmoothingMode = Graphics.SmoothingMode;
                Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                Graphics.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
                Graphics.SmoothingMode = oldSmoothingMode;
            }
        }

        public void DrawOffscreen(IOffscreen offscreen, Point2D pt)
        {
            var screen = offscreen.DownCast<Offscreen>();
            screen.Flush();
            var image = screen.Image;
            Graphics.DrawImage(image, pt.X, pt.Y, image.Width, image.Height);
        }
    }
}
