using System;
using System.Windows.Forms;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;

namespace Sample.WinForms.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CanvasContext : ICanvasContext, IAutoFieldDisposable
    {
        public event Action<Size2D>? Created;

        public event Action<Size2D>? Resize;

        public event Action<ICanvasContext.IRenderingContext>? RenderFrame;

        public event Action? GotFocus;

        public event Action? LostFocus;

        public event Action<MouseState>? MouseDown;

        public event Action<MouseState>? MouseUp;

        public event Action<MouseState>? MouseMove;

        public event Action<MouseState>? MouseWheel;

        [Aggregation]
        private readonly TextEditorControl _control;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public CanvasContext(TextEditorControl control)
        {
            _control = control;
            _control.HandleCreated += (_, __) =>
            {
                _control.Resize += (s, e) => Resize?.Invoke(new Size2D(_control.ClientSize.Width, _control.ClientSize.Height));
                _control.GotFocus += (s, e) => GotFocus?.Invoke();
                _control.LostFocus += (s, e) => LostFocus?.Invoke();
                _control.Paint += (s, e) => RenderFrame?.Invoke(new RenderingContext(e.Graphics));
                _control.MouseDown += (s, e) => MouseDown?.Invoke(ToMouseEventInfo(e));
                _control.MouseUp += (s, e) => MouseUp?.Invoke(ToMouseEventInfo(e));
                _control.MouseMove += (s, e) => MouseMove?.Invoke(ToMouseEventInfo(e));
                _control.MouseWheel += (s, e) => MouseWheel?.Invoke(ToMouseEventInfo(e));

                Created?.Invoke(new Size2D(_control.ClientSize.Width, _control.ClientSize.Height));
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public IOffscreen CreateOffscreen(Size2D size)
        {
            return new Offscreen(size);
        }

        /// <summary>
        /// 
        /// </summary>
        public IFont CreateFont(string fontName, double fontHeight)
        {
            return new Font(fontName, fontHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        public IInputMethod GetInputMethod()
        {
            return _control.InputMethod;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestRenderFrame()
        {
            _control.Invalidate();
        }

        public void SetCursor(Crash.Core.UI.UIContext.Cursor cursor)
        {
            _control.Cursor = cursor switch
            {
                Crash.Core.UI.UIContext.Cursor.Default => Cursors.Default,
                Crash.Core.UI.UIContext.Cursor.Text => Cursors.IBeam,
                _ => throw new InvalidOperationException(),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private static MouseState ToMouseEventInfo(MouseEventArgs e)
        {
            const double WheelDeltaOfNotch = 120.0;
            return new MouseState(
                e.Location.X,
                e.Location.Y,
                Convert.ToInt32(Math.Ceiling(e.Delta / WheelDeltaOfNotch)),
                e.Button.HasFlag(System.Windows.Forms.MouseButtons.Left),
                e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right),
                e.Button.HasFlag(System.Windows.Forms.MouseButtons.Middle),
                Control.ModifierKeys.HasFlag(Keys.Control),
                Control.ModifierKeys.HasFlag(Keys.Shift),
                Control.ModifierKeys.HasFlag(Keys.Alt));
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class RenderingContext : ICanvasContext.IRenderingContext
        {
            [Aggregation]
            private readonly System.Drawing.Graphics _g;

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public RenderingContext(System.Drawing.Graphics graphics)
            {
                _g = graphics;
            }

            /// <summary>
            /// 
            /// </summary>
            public void TransferScreen(IOffscreen offscreen)
            {
                var screen = offscreen.DownCast<Offscreen>();
                screen.Flush();
                var image = screen.Image;
                _g.DrawImage(image, 0, 0, image.Width, image.Height);
            }
        }
    }
}
