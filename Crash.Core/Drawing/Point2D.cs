using System;

namespace Crash.Core.Drawing
{
    using Myself = Point2D;

    /// <summary>
    /// 座標を表す。
    /// </summary>
    public readonly struct Point2D : IEquatable<Point2D>
    {
        public static readonly Point2D Origin = default;

        public bool IsOrigin => this == Origin;

        private readonly (int x, int y) _value;

        public int X => _value.x;

        public int Y => _value.y;

        public Point2D(int x, int y)
        {
            _value = (x, y);
        }

        public Size2D ToSize()
        {
            return new Size2D(X, Y);
        }

        public Point2D Offset(int width, int height)
        {
            return new Point2D(X + width, Y + height);
        }

        public Point2D Offset(Size2D size)
        {
            return Offset(size.Width, size.Height);
        }

        public Point2D OffsetX(int x)
        {
            return Offset(x, 0);
        }

        public Point2D OffsetY(int y)
        {
            return Offset(0, y);
        }

        public Size2D Subtract(Point2D location)
        {
            return new Size2D(X - location.X, Y - location.Y);
        }

        public Point2D InvertSign()
        {
            return new Point2D(-X, -Y);
        }

        public Point2D Clamp(Rect2D rect)
        {
            Verifier.Verify<ArgumentException>(rect.IsValid);

            return new Point2D(
                MathUtil.Clamp(X, rect.Left, rect.Right - 1),
                MathUtil.Clamp(Y, rect.Top, rect.Bottom - 1));
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
