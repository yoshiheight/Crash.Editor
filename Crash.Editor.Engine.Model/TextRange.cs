using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Crash.Core;

namespace Crash.Editor.Engine.Model
{
    using Myself = TextRange;

    /// <summary>
    /// テキストの範囲を表す。
    /// </summary>
    public readonly struct TextRange : IEquatable<TextRange>
    {
        private readonly ((TextPos start, TextPos end) originalRange, bool isNormal, RangeKind rangeKind) _value;

        public TextPos OriginalStart => _value.originalRange.start;

        public TextPos OriginalEnd => _value.originalRange.end;

        public TextPos Start => _value.isNormal ? _value.originalRange.start : _value.originalRange.end;

        public TextPos End => _value.isNormal ? _value.originalRange.end : _value.originalRange.start;

        public bool IsEmpty => OriginalStart == OriginalEnd;

        public int LineCount => LineOffset + 1;

        public int LineOffset => End.LineIndex - Start.LineIndex;

        public int LineIndex => LineCount == 1 ? Start.LineIndex : throw new InvalidOperationException();

        public int CharLength => LineCount == 1 ? End.CharIndex - Start.CharIndex : throw new InvalidOperationException();

        public TextRange(TextPos start, TextPos end, RangeKind rangeKind = RangeKind.Normal)
        {
            _value = ((start, end), start <= end, rangeKind);
        }

        public TextRange(int startLineIndex, int startCharIndex, int endLineIndex, int endCharIndex, RangeKind rangeKind = RangeKind.Normal)
            : this(new TextPos(startLineIndex, startCharIndex), new TextPos(endLineIndex, endCharIndex), rangeKind)
        { }

        public bool Contains(in TextRange range)
        {
            return Start <= range.Start && range.End <= End;
        }

        public IEnumerable<int> GetLineIndexes()
        {
            return MathUtil.RangeStartToEndInclusive(Start.LineIndex, End.LineIndex);
        }

        #region 等値比較
        public override int GetHashCode() => _value.GetHashCode();
        public override bool Equals(object? obj) => obj is Myself other && Equals(other);
        public bool Equals(Myself other) => _value.Equals(other._value);
        public static bool operator ==(Myself left, Myself right) => left.Equals(right);
        public static bool operator !=(Myself left, Myself right) => !left.Equals(right);
        #endregion
    }

    // TODO 以下のテキスト範囲種別による処理は未実装

    /// <summary>
    /// テキスト範囲種別。
    /// </summary>
    public enum RangeKind : byte
    {
        /// <summary>通常。</summary>
        Normal,

        /// <summary>矩形。</summary>
        Rect,
    }
}
