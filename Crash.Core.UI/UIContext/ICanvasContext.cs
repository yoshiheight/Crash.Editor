using System;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;

namespace Crash.Core.UI.UIContext
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICanvasContext : IDisposable
    {
        event Action<Size2D> Created;

        event Action<Size2D> Resize;

        event Action<IRenderingContext> RenderFrame;

        event Action<MouseState> MouseDown;

        event Action<MouseState> MouseUp;

        event Action<MouseState> MouseMove;

        event Action<MouseState> MouseWheel;

        event Action GotFocus;

        event Action LostFocus;

        IOffscreen CreateOffscreen(Size2D size);

        IFont CreateFont(string family, double height);

        IInputMethod GetInputMethod() => throw new NotImplementedException();

        void RequestRenderFrame();

        void SetCursor(Cursor cursor);

        /// <summary>
        /// 
        /// </summary>
        public interface IRenderingContext
        {
            void TransferScreen(IOffscreen offscreen);
        }
    }

    public enum Cursor
    {
        Default,
        Text,
    }
}
