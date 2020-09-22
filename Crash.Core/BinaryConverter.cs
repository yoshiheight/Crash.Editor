using System;
using System.Collections.Generic;
using System.Globalization;

namespace Crash.Core
{
    /// <summary>
    /// バイナリと別な型との相互変換を行う。
    /// </summary>
    public static class BinaryConverter
    {
        /// <summary>
        /// byteコレクションを16進数文字列に変換する。
        /// </summary>
        public static string BytesToHexString(IReadOnlyList<byte> bytes, HexFormat format)
        {
            Span<char> chars = stackalloc char[bytes.Count * 2];
            for (var i = 0; i < bytes.Count; i++)
            {
                var t = ByteToHexChar(bytes[i], format);
                chars[i * 2] = t.high;
                chars[i * 2 + 1] = t.low;
            }
            return new string(chars);
        }

        /// <summary>
        /// 16進数文字列をbyte配列に変換する。
        /// </summary>
        public static byte[] HexStringToBytes(IReadOnlyList<char> chars)
        {
            var bytes = new byte[chars.Count / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = HexCharToByte(chars[i * 2], chars[i * 2 + 1]);
            }
            return bytes;
        }

        /// <summary>
        /// byteを16進数文字に変換する。
        /// </summary>
        public static (char high, char low) ByteToHexChar(byte value, HexFormat format)
        {
            Span<char> chars = stackalloc char[2];
            return value.TryFormat(chars, out _, format.GetStringValue()) ? (chars[0], chars[1]) : throw new InvalidOperationException();
        }

        /// <summary>
        /// 16進数文字をbyteに変換する。
        /// </summary>
        public static byte HexCharToByte(char high, char low)
        {
            return byte.Parse(stackalloc[] { high, low }, NumberStyles.AllowHexSpecifier);
        }

        /// <summary>
        /// byte要素のValueTupleをuintに変換する。
        /// </summary>
        public static uint ValueTupleToUInt32((byte, byte, byte, byte) value)
        {
            return (uint)(value.Item1 << 24 | value.Item2 << 16 | value.Item3 << 8 | value.Item4);
        }

        /// <summary>
        /// uintをbyte要素のValueTupleに変換する。
        /// </summary>
        public static (byte, byte, byte, byte) UInt32ToValueTuple(uint value)
        {
            return ((byte)(value >> 24), (byte)(value >> 16 & 0xFF), (byte)(value >> 8 & 0xFF), (byte)(value & 0xFF));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum HexFormat
    {
        [StringValue("x2")]
        Lower,

        [StringValue("X2")]
        Upper,
    }
}
