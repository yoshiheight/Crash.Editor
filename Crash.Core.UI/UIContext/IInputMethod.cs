using System;
using Crash.Core.Drawing;

namespace Crash.Core.UI.UIContext
{
    public interface IInputMethod : IDisposable
    {
        void SetFont(string fontName, double fontHeight, int lineHeight);

        void SetArea(Rect2D textArea, Point2D caretLocation);
    }
}
