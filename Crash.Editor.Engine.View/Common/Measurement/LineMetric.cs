namespace Crash.Editor.Engine.View.Common.Measurement
{
    public interface ILineMetric
    {
        int Index { get; }
        int Y { get; }
        int Height { get; }
    }

    public sealed class LineMetric : ILineMetric
    {
        public int Index { get; private set; }
        public int Y { get; private set; }
        public int Height { get; private set; }

        public LineMetric Init(int index, int y, int height)
        {
            Index = index;
            Y = y;
            Height = height;

            return this;
        }
    }
}
