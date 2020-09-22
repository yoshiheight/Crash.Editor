using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.Model;

namespace Crash.Editor.Engine.View.Common.Measurement
{
#warning とりあえず
    public sealed class CharMetric : ICharMetric
    {
        public int LineIndex { get; private set; }

        public int LineHeight { get; private set; }

        public char Char { get; private set; }

        public double Left { get; private set; }

        public double Right { get; private set; }

        public int IndexEachLine { get; private set; }

        public int OriginalIndexEachLine { get; private set; }

        public int ColumnLen { get; private set; }

        public Color Color { get; private set; }

        public Color BackgroundColor { get; private set; }

        public IFont Font { get; private set; }

        public double Width { get; private set; }

        public Rect2D CharRect { get; private set; }

        public TextRange CharRange { get; private set; }

        public int Y { get; private set; }

#warning とりあえず
        /// <summary>
        /// 
        /// </summary>
        public CharMetric Init(
            int lineIndex, int lineHeight,
            char ch, double x,
            int index, int originalIndex, int columnLen,
            Color color, Color backgroundColor,
            IFont fontId, double width, double adjustY)
        {
            LineIndex = lineIndex;
            LineHeight = lineHeight;
            Char = ch;
            IndexEachLine = index;
            OriginalIndexEachLine = originalIndex;
            ColumnLen = columnLen;
            Color = color;
            BackgroundColor = backgroundColor;
            Font = fontId;
            Width = width;
            Left = x;
            Right = Left + Width;
            Y = MathUtil.RoundAwayFromZeroToInt32(lineIndex * lineHeight + adjustY);
            CharRect = new Rect2D(
                    (int)x,
                    lineIndex * lineHeight,
                    CharSize.RoundWidth(width),
                    lineHeight);
            CharRange = new TextRange(
                lineIndex, originalIndex,
                lineIndex, originalIndex + 1);

            return this;
        }
    }

    public interface ICharMetric
    {
        int LineIndex { get; }

        int LineHeight { get; }

        char Char { get; }

        double Left { get; }

        double Right { get; }

        int IndexEachLine { get; }

        //int OriginalIndexEachLine { get; }

        int ColumnLen { get; }

        Color Color { get; }

        Color BackgroundColor { get; }

        IFont Font { get; }

        double Width { get; }

        Rect2D CharRect { get; }

        TextRange CharRange { get; }

        int Y { get; }
    }
}
