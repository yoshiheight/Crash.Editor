using System;
using Crash.Core.Collections;

namespace Crash.Editor.Engine.Model
{
    using Myself = TextPos;

    /// <summary>
    /// エディタ上の位置を表す。
    /// </summary>
    public readonly struct TextPos : IEquatable<TextPos>, IComparable<TextPos>
    {
        private readonly (int lineIndex, int charIndex) _value;

        public static readonly TextPos Empty;

        /// <summary>行インデックス。</summary>
        public int LineIndex => _value.lineIndex;

        /// <summary>文字インデックス。</summary>
        public int CharIndex => _value.charIndex;

        public TextPos(int lineIndex, int charIndex)
        {
            _value = (lineIndex, charIndex);
        }

        public void Deconstruct(out int lineIndex, out int charIndex)
        {
            (lineIndex, charIndex) = _value;
        }

        /// <summary>
        /// 
        /// </summary>
        public TextPos GetNextPos(IReadOnlyTextDocument doc)
        {
            return (this == GetEndPos(doc)) ? this
                : (CharIndex < doc.Lines[LineIndex].Length) ? new TextPos(LineIndex, CharIndex + 1)
                : new TextPos(LineIndex + 1, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public TextPos GetPrevPos(IReadOnlyTextDocument doc)
        {
            return (this == GetTopPos()) ? this
                : (CharIndex > 0) ? new TextPos(LineIndex, CharIndex - 1)
                : new TextPos(LineIndex - 1, doc.Lines[LineIndex - 1].Length);
        }

        /// <summary>
        /// 
        /// </summary>
        public static TextPos GetTopPos()
        {
            return Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public static TextPos GetEndPos(IReadOnlyTextDocument doc)
        {
            var lastIndex = doc.Lines.LastIndex();
            return new TextPos(lastIndex, doc.Lines[lastIndex].Length);
        }

        #region 等値比較
        public override int GetHashCode() => _value.GetHashCode();
        public override bool Equals(object? obj) => obj is Myself other && Equals(other);
        public bool Equals(Myself other) => _value.Equals(other._value);
        public static bool operator ==(Myself left, Myself right) => left.Equals(right);
        public static bool operator !=(Myself left, Myself right) => !left.Equals(right);
        #endregion

        #region 大小比較
        public int CompareTo(Myself other) => _value.CompareTo(other._value);
        public static bool operator >(Myself left, Myself right) => left.CompareTo(right) > 0;
        public static bool operator <(Myself left, Myself right) => left.CompareTo(right) < 0;
        public static bool operator >=(Myself left, Myself right) => left.CompareTo(right) >= 0;
        public static bool operator <=(Myself left, Myself right) => left.CompareTo(right) <= 0;
        #endregion
    }
}
