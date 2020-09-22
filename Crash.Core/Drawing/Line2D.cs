using System;

namespace Crash.Core.Drawing
{
    using Myself = Line2D;

    /// <summary>
    /// 線を表す。
    /// </summary>
    public readonly struct Line2D : IEquatable<Line2D>
    {
        /// <summary></summary>
        public static readonly Line2D Empty = default;

        private readonly (Point2D from, Point2D to) _value;

        /// <summary>開始座標。</summary>
        public Point2D From => _value.from;

        /// <summary>終了座標（線を描画した場合、終了座標は描画されない）。</summary>
        public Point2D To => _value.to;

        /// <summary>線が無効な状態かどうか。</summary>
        public bool IsInvalid => From == To;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Line2D(Point2D from, Point2D to)
        {
            _value = (from, to);
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
