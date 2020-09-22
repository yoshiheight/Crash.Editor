using System;

namespace Crash.Core.Drawing
{
    using Myself = Rect2D;

    /// <summary>
    /// 矩形を表す。
    /// </summary>
    public readonly struct Rect2D : IEquatable<Rect2D>
    {
        /// <summary></summary>
        public static readonly Rect2D Empty = default;

        private readonly (Point2D location, Size2D size) _value;

        /// <summary></summary>
        public Point2D Location => _value.location;

        /// <summary></summary>
        public Size2D Size => _value.size;

        /// <summary>矩形が有効な状態かどうか。</summary>
        public bool IsValid => Width > 0 && Height > 0;

        /// <summary>矩形が無効な状態かどうか。</summary>
        public bool IsInvalid => !IsValid;

        /// <summary>X座標。</summary>
        public int X => _value.location.X;

        /// <summary>Y座標。</summary>
        public int Y => _value.location.Y;

        /// <summary>幅。</summary>
        public int Width => _value.size.Width;

        /// <summary>高さ。</summary>
        public int Height => _value.size.Height;

        /// <summary>左辺。</summary>
        public int Left => _value.location.X;

        /// <summary>上辺。</summary>
        public int Top => _value.location.Y;

        /// <summary>右辺（矩形を塗りつぶした場合、右辺は描画されない）。</summary>
        public int Right => _value.location.X + _value.size.Width;

        /// <summary>下辺（矩形を塗りつぶした場合、下辺は描画されない）。</summary>
        public int Bottom => _value.location.Y + _value.size.Height;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Rect2D(Point2D location, Size2D size)
        {
            _value = (location, size);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Rect2D(int x, int y, int width, int height)
            : this(new Point2D(x, y), new Size2D(width, height))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public static Rect2D FromLTRB(int l, int t, int r, int b)
        {
            return new Rect2D(l, t, r - l, b - t);
        }

        public Rect2D Inflate(int width, int height)
        {
            return new Rect2D(Location.Offset(-width, -height), Size.Add(width * 2, height * 2));
        }

        public Rect2D Offset(int x, int y)
        {
            return Offset(new Size2D(x, y));
        }

        public Rect2D Offset(Size2D size)
        {
            return new Rect2D(Location.Offset(size), Size);
        }

        public Rect2D OffsetX(int x)
        {
            return Offset(x, 0);
        }

        public Rect2D OffsetY(int y)
        {
            return Offset(0, y);
        }

        public bool IntersectsWith(Rect2D rect)
        {
            return IsValid && rect.IsValid && (rect.Left < Right && rect.Right > Left && rect.Top < Bottom && rect.Bottom > Top);
        }

        public Rect2D Intersect(Rect2D rect)
        {
            return IntersectsWith(rect) ? Rect2D.FromLTRB(Math.Max(Left, rect.Left), Math.Max(Top, rect.Top), Math.Min(Right, rect.Right), Math.Min(Bottom, rect.Bottom))
                : Empty;
        }

        public Rect2D Union(Rect2D rect)
        {
            return (IsValid && rect.IsValid) ? Rect2D.FromLTRB(Math.Min(Left, rect.Left), Math.Min(Top, rect.Top), Math.Max(Right, rect.Right), Math.Max(Bottom, rect.Bottom))
                : IsValid ? this
                : rect.IsValid ? rect
                : Empty;
        }

        public bool Contains(Point2D location)
        {
            return Contains(location.X, location.Y);
        }

        public bool Contains(int x, int y)
        {
            return Left <= x && x < Right && Top <= y && y < Bottom;
        }

        public bool Contains(Rect2D rect)
        {
            return rect.Left >= Left && rect.Right <= Right && rect.Top >= Top && rect.Bottom <= Bottom;
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
