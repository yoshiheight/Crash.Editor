using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.Text;
using Crash.Core.UI.UIContext;
using Sample.BlazorWasm.TextEditor.Common.Interop;

namespace Sample.BlazorWasm.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputMethod : IInputMethod, IAutoFieldDisposable
    {
        [DisposableField]
        private readonly JSObjectReference _jsInputMethod;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public InputMethod(JSObjectReference jsRefTextEditorComponent)
        {
            _jsInputMethod = JSObjectReference.From(jsRefTextEditorComponent, "bindInputMethod");
        }

        /// <summary>
        /// 
        /// </summary>
        internal void SetClipboardText(string text)
        {
            var bytes = TextEncodes.UTF8NoBOM.GetBytes(text);
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    _jsInputMethod.CallVoid("setClipboardText", (ulong)ptr, bytes.Length);
                }
            }
        }

        /// <summary>
        /// フォント設定。
        /// </summary>
        public void SetFont(string family, double height, int lineHeight)
        {
            _jsInputMethod.CallVoid("setFont", family, height, lineHeight);
        }

        /// <summary>
        /// キャレット位置及び領域設定。
        /// </summary>
        public void SetArea(Rect2D area, Point2D caretLocation)
        {
            if (area.IsInvalid) return;

            caretLocation = caretLocation.Clamp(area);

            _jsInputMethod.CallVoid("setArea",
                new { x = area.X, y = area.Y, w = area.Width, h = area.Height },
                new { x = caretLocation.X, y = caretLocation.Y });
        }
    }
}
