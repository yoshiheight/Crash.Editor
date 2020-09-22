using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI.UIContext;
using Microsoft.JSInterop;
using Sample.BlazorWasm.TextEditor.Common.Interop;

namespace Sample.BlazorWasm.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Offscreen : IOffscreen, IAutoFieldDisposable
    {
        [DisposableField]
        private readonly JSObjectReference _jsOffscreen;

        [DisposableField(Order = 1)]
        private readonly MemoryStream _utf8jsonMemory = new MemoryStream();

        [DisposableField(Order = 0)]
        private readonly Utf8JsonWriter _writer;

        public JSObjectReference JsObj => _jsOffscreen;

        private enum JSRenderFuncName
        {
            save,
            restore,
            clip,
            setColor,
            setFont,
            drawString,
            fillRect,
            fillPolygon,
            drawOffscreen,
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Offscreen(Size2D size, IJSInProcessRuntime jsRuntime)
        {
            _jsOffscreen = JSObjectReference.From(jsRuntime,
                "Sample.BlazorWasm.TextEditor.Offscreen.create", new { w = size.Width, h = size.Height });

            _writer = new Utf8JsonWriter(_utf8jsonMemory);

            Init();
        }

        private void Init()
        {
            _utf8jsonMemory.Seek(0, SeekOrigin.Begin);

            _writer.Reset();
            _writer.WriteStartObject();
            _writer.WriteStartArray("funcs");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resize(Size2D size)
        {
            _jsOffscreen.CallVoid("resize", new { w = size.Width, h = size.Height });
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Flush()
        {
            _writer.WriteEndArray();
            _writer.WriteEndObject();
            _writer.Flush();

            unsafe
            {
                fixed (byte* ptr = _utf8jsonMemory.GetBuffer())
                {
                    _jsOffscreen.CallVoid("callCommands", (ulong)ptr, _utf8jsonMemory.Position);
                }
            }

            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public IDisposable Save()
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.save));
            _writer.WriteEndObject();

            return Disposable.Create(() => restore());

            void restore()
            {
                _writer.WriteStartObject();
                _writer.WriteString("name", nameof(JSRenderFuncName.restore));
                _writer.WriteEndObject();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clip(IReadOnlyRegion2D region)
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.clip));
            _writer.WriteStartArray("arg");
            foreach (var rect in region.Rects)
            {
                _writer.WriteStartObject();
                _writer.WriteNumber("x", rect.X);
                _writer.WriteNumber("y", rect.Y);
                _writer.WriteNumber("w", rect.Width);
                _writer.WriteNumber("h", rect.Height);
                _writer.WriteEndObject();
            }
            _writer.WriteEndArray();
            _writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetColor(Color color)
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.setColor));
            _writer.WriteString("arg", color.ToStringHashRgba(stackalloc char[Color.LengthOfStringHashRgba]));
            _writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetFont(IFont font)
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.setFont));
            _writer.WriteNumber("arg", font.DownCast<Font>().JsObj.Id);
            _writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DrawString(ReadOnlySpan<char> text, Point2D pt)
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.drawString));
            _writer.WriteStartObject("arg");
            _writer.WriteString("str", text);
            _writer.WriteStartObject("pt");
            _writer.WriteNumber("x", pt.X);
            _writer.WriteNumber("y", pt.Y);
            _writer.WriteEndObject();
            _writer.WriteEndObject();
            _writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        public void FillPolygon(IEnumerable<Point2D> points)
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.fillPolygon));
            _writer.WriteStartArray("arg");
            foreach (var pt in points)
            {
                _writer.WriteStartObject();
                _writer.WriteNumber("x", pt.X);
                _writer.WriteNumber("y", pt.Y);
                _writer.WriteEndObject();
            }
            _writer.WriteEndArray();
            _writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        public void FillRect(Rect2D rect)
        {
            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.fillRect));
            _writer.WriteStartObject("arg");
            _writer.WriteNumber("x", rect.X);
            _writer.WriteNumber("y", rect.Y);
            _writer.WriteNumber("w", rect.Width);
            _writer.WriteNumber("h", rect.Height);
            _writer.WriteEndObject();
            _writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DrawOffscreen(IOffscreen offscreen, Point2D pt)
        {
            var screen = offscreen.DownCast<Offscreen>();
            screen.Flush();

            _writer.WriteStartObject();
            _writer.WriteString("name", nameof(JSRenderFuncName.drawOffscreen));
            _writer.WriteStartObject("arg");
            _writer.WriteNumber("id", screen.JsObj.Id);
            _writer.WriteStartObject("pt");
            _writer.WriteNumber("x", pt.X);
            _writer.WriteNumber("y", pt.Y);
            _writer.WriteEndObject();
            _writer.WriteEndObject();
            _writer.WriteEndObject();
        }
    }
}
