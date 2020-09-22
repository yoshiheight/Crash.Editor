using System;
using System.Text.Json;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;
using Microsoft.JSInterop;
using Sample.BlazorWasm.TextEditor.Common.Interop;
using Sample.BlazorWasm.TextEditor.Shared;

namespace Sample.BlazorWasm.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CanvasContext : ICanvasContext, IAutoFieldDisposable
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
        private readonly TextEditorComponent _textEditorComponent;

        [Aggregation]
        private readonly IJSInProcessRuntime _jsRuntime;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public CanvasContext(TextEditorComponent textEditorComponent, IJSInProcessRuntime jsRuntime, JSEvent jsEvent)
        {
            _textEditorComponent = textEditorComponent;
            _jsRuntime = jsRuntime;

            _textEditorComponent.Created += size =>
            {
                jsEvent.RegisterActionListener("TextEditorComponent.resize", args => Resize?.Invoke(ToSize(args)));
                jsEvent.RegisterActionListener("TextEditorComponent.mousedown", args => MouseDown?.Invoke(ToMouseState(args)));
                jsEvent.RegisterActionListener("TextEditorComponent.mouseup", args => MouseUp?.Invoke(ToMouseState(args)));
                jsEvent.RegisterActionListener("TextEditorComponent.mousemove", args => MouseMove?.Invoke(ToMouseState(args)));
                jsEvent.RegisterActionListener("TextEditorComponent.mousewheel", args => MouseWheel?.Invoke(ToMouseState(args)));
                jsEvent.RegisterActionListener("TextEditorComponent.render", _ => RenderFrame?.Invoke(new RenderingContext(_textEditorComponent)));
                jsEvent.RegisterActionListener("InputMethod.focusin", args => GotFocus?.Invoke());
                jsEvent.RegisterActionListener("InputMethod.focusout", args => LostFocus?.Invoke());

                Created?.Invoke(size);
            };
        }

        private static Point2D ToPoint(JsonElement elem)
        {
            return new Point2D(
                elem.GetProperty("x").GetInt32(),
                elem.GetProperty("y").GetInt32());
        }

        private static Size2D ToSize(JsonElement elem)
        {
            return new Size2D(
                elem.GetProperty("w").GetInt32(),
                elem.GetProperty("h").GetInt32());
        }

        private static MouseState ToMouseState(JsonElement args)
        {
#warning あとで見直し
            var button = args.GetProperty("button").GetInt32();
            return new MouseState(
                args.GetProperty("x").GetInt32(),
                args.GetProperty("y").GetInt32(),
                args.GetProperty("notchY").GetInt32(),
                button == 0,
                button == 2,
                button == 1,
                args.GetProperty("isCtrl").GetBoolean(),
                args.GetProperty("isShift").GetBoolean(),
                args.GetProperty("isAlt").GetBoolean());
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestRenderFrame()
        {
            _textEditorComponent.RequestRender();
        }

        /// <summary>
        /// 
        /// </summary>
        public IOffscreen CreateOffscreen(Size2D size)
        {
            return new Offscreen(size, _jsRuntime);
        }

        public IFont CreateFont(string family, double height)
        {
            return new Font(family, height, _jsRuntime);
        }

        public IInputMethod GetInputMethod()
        {
            return _textEditorComponent.InputMethod;
        }

        public void SetCursor(Cursor cursor)
        {
            _textEditorComponent.SetCursor(cursor switch
            {
                Cursor.Default => JSCursor.@default,
                Cursor.Text => JSCursor.text,
                _ => throw new InvalidOperationException(),
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class RenderingContext : ICanvasContext.IRenderingContext
        {
            [Aggregation]
            private readonly TextEditorComponent _textEditorComponent;

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public RenderingContext(TextEditorComponent textEditorComponent)
            {
                _textEditorComponent = textEditorComponent;
            }

            /// <summary>
            /// 
            /// </summary>
            public void TransferScreen(IOffscreen offscreen)
            {
                var screen = offscreen.DownCast<Offscreen>();
                screen.Flush();
                _textEditorComponent.TransferScreen(screen.JsObj);
            }
        }
    }
}
