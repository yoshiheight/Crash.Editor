using System;

namespace Crash.Core.Drawing
{
    using Myself = Size2D;

    /// <summary>
    /// サイズを表す。
    /// </summary>
    public readonly struct Size2D : IEquatable<Size2D>
    {
        public static readonly Size2D Empty = default;

        private readonly (int width, int height) _value;

        public bool IsEmpty => this == Empty;

        public int Width => _value.width;

        public int Height => _value.height;

        public Size2D(int width, int height)
        {
            _value = (width, height);
        }

        public Size2D(double width, double height)
            : this((int)width, (int)height)
        {
        }

        public Size2D Add(int width, int height)
        {
            return Add(new Size2D(width, height));
        }

        public Size2D Add(Size2D size)
        {
            return new Size2D(Width + size.Width, Height + size.Height);
        }

        public Size2D InvertSign()
        {
            return new Size2D(-Width, -Height);
        }

        public Size2D ClampMin(int minWidth, int minHeight)
        {
            return new Size2D(
                MathUtil.ClampMin(Width, minWidth),
                MathUtil.ClampMin(Height, minHeight));
        }

        public Point2D ToPoint()
        {
            return new Point2D(Width, Height);
        }

        #region 等値比較
        public override int GetHashCode() => _value.GetHashCode();
        public override bool Equals(object? obj) => obj is Myself other && Equals(other);
        public bool Equals(Myself other) => _value.Equals(other._value);
        public static bool operator ==(Myself left, Myself right) => left.Equals(right);
        public static bool operator !=(Myself left, Myself right) => !left.Equals(right);
        #endregion
    }
}
