using System;

namespace Crash.Core.Drawing
{
    using Myself = Color;

    /// <summary>
    /// 色を表す。
    /// </summary>
    public readonly struct Color : IEquatable<Color>
    {
        public static readonly int LengthOfStringHashRgba = 9;

        private readonly (byte r, byte g, byte b, byte a) _value;

        /// <summary>赤。</summary>
        public byte R => _value.r;

        /// <summary>緑。</summary>
        public byte G => _value.g;

        /// <summary>青。</summary>
        public byte B => _value.b;

        /// <summary>アルファ。</summary>
        public byte A => _value.a;

        public bool IsTransparent => A == 0;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        private Color((byte r, byte g, byte b, byte a) value)
        {
            _value = value;
        }

        /// <summary>
        /// RGBA値を基に生成する。
        /// </summary>
        public static Color FromRgba(byte r, byte g, byte b, byte a)
        {
            return new Color((r, g, b, a));
        }

        /// <summary>
        /// RGB値を基に生成する。
        /// </summary>
        public static Color FromRgb(byte r, byte g, byte b)
        {
            return new Color((r, g, b, 0xFF));
        }

        /// <summary>
        /// RGBA値を基に生成する。
        /// </summary>
        public static Color FromRgba(uint rgba)
        {
            return new Color(BinaryConverter.UInt32ToValueTuple(rgba));
        }

        /// <summary>
        /// RGB値を基に生成する。
        /// </summary>
        public static Color FromRgb(uint rgb)
        {
            return FromRgba(rgb << 8 | 0xFF);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color FromStringHashRgbaOrRgb(ReadOnlySpan<char> colorString)
        {
            Verifier.Verify<ArgumentException>(colorString.StartsWith("#"));
            Verifier.Verify<ArgumentException>(colorString.Length == 7 || colorString.Length == 9);

            return Color.FromRgba(
                BinaryConverter.HexCharToByte(colorString[1], colorString[2]),
                BinaryConverter.HexCharToByte(colorString[3], colorString[4]),
                BinaryConverter.HexCharToByte(colorString[5], colorString[6]),
                (colorString.Length == 9) ? BinaryConverter.HexCharToByte(colorString[7], colorString[8]) : (byte)0xFF);
        }

        /// <summary>
        /// 
        /// </summary>
        public Span<char> ToStringHashRgba(Span<char> destination)
        {
            var r = BinaryConverter.ByteToHexChar(R, HexFormat.Lower);
            var g = BinaryConverter.ByteToHexChar(G, HexFormat.Lower);
            var b = BinaryConverter.ByteToHexChar(B, HexFormat.Lower);
            var a = BinaryConverter.ByteToHexChar(A, HexFormat.Lower);

            destination[0] = '#';
            destination[1] = r.high;
            destination[2] = r.low;
            destination[3] = g.high;
            destination[4] = g.low;
            destination[5] = b.high;
            destination[6] = b.low;
            destination[7] = a.high;
            destination[8] = a.low;

            return destination;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ToStringHashRgba()
        {
            return string.Create(LengthOfStringHashRgba, this, (buffer, self) => self.ToStringHashRgba(buffer));
        }

        #region 等値比較
        public override int GetHashCode() => _value.GetHashCode();
        public override bool Equals(object? obj) => obj is Myself other && Equals(other);
        public bool Equals(Myself other) => _value.Equals(other._value);
        public static bool operator ==(Myself left, Myself right) => left.Equals(right);
        public static bool operator !=(Myself left, Myself right) => !left.Equals(right);
        #endregion

        // 以下、色定数定義

        public static readonly Color DimGray = FromRgb(0x696969);

        public static readonly Color CadetBlue = FromRgb(0x5F9EA0);

        public static readonly Color SandyBrown = FromRgb(0xF4A460);

        public static readonly Color DarkGray = FromRgb(0xA9A9A9);

        public static readonly Color DarkCyan = FromRgb(0x008B8B);

        public static readonly Color Red = FromRgb(0xFF0000);

        public static readonly Color Blue = FromRgb(0x0000FF);

        public static readonly Color Gray = FromRgb(0x808080);

        public static readonly Color White = FromRgb(0xFFFFFF);

        public static readonly Color Black = FromRgb(0x000000);

        public static readonly Color Transparent = FromRgba(0x00000000);
    }
}
