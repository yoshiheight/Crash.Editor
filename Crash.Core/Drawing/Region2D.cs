using System.Collections.Generic;
using System.Linq;

namespace Crash.Core.Drawing
{
    public sealed class Region2D : IReadOnlyRegion2D
    {
        private readonly HashSet<Rect2D> _rectSet = new HashSet<Rect2D>();

        IEnumerable<Rect2D> IReadOnlyRegion2D.Rects => _rectSet;

        private Rect2D _bounds;

        Rect2D IReadOnlyRegion2D.Bounds => _bounds;

        public IReadOnlyRegion2D AsReadOnly() => this;

        public Region2D AddRect(Rect2D rect)
        {
            if (rect.IsValid)
            {
                _rectSet.Add(rect);
                _bounds = _bounds.Union(rect);
            }
            return this;
        }

        bool IReadOnlyRegion2D.IntersectsWith(Rect2D targetRect)
        {
            return _bounds.IntersectsWith(targetRect) && _rectSet.Any(rect => rect.IntersectsWith(targetRect));
        }

        IReadOnlyRegion2D IReadOnlyRegion2D.Offset(Size2D size)
        {
            var region = new Region2D();
            foreach (var rect in _rectSet.Select(rect => rect.Offset(size)))
            {
                region.AddRect(rect);
            }
            return region;
        }

        IReadOnlyRegion2D IReadOnlyRegion2D.Mask(Rect2D maskRect)
        {
            var region = new Region2D();
            foreach (var rect in _rectSet.Select(rect => maskRect.Intersect(rect)))
            {
                region.AddRect(rect);
            }
            return region;
        }

        public void Clear()
        {
            _rectSet.Clear();
            _bounds = Rect2D.Empty;
        }
    }

    public interface IReadOnlyRegion2D
    {
        IEnumerable<Rect2D> Rects { get; }

        Rect2D Bounds { get; }

        bool IntersectsWith(Rect2D rect);

        IReadOnlyRegion2D Offset(Size2D size);

        IReadOnlyRegion2D Mask(Rect2D maskRect);
    }
}
