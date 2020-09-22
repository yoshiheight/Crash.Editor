using System;

namespace Crash.Core.UI.Common
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct CharSize
    {
        public double Width { get; }
        public double Height { get; }

        public bool IsEmpty => Width == 0.0 || Height == 0.0;

        public CharSize(double width, double height)
        {
            Verifier.Verify<ArgumentException>(width > 0.0 && height > 0.0);

            Width = width;
            Height = height;
        }

        public static int RoundWidth(double width)
        {
            return Convert.ToInt32(Math.Ceiling(width));
        }

        public static int RoundHeight(double height)
        {
            return Convert.ToInt32(Math.Ceiling(height));
        }
    }
}
