using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Crash.Core;
using Crash.Core.Collections;
using Crash.Core.Text;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;
using Microsoft.JSInterop;
using Sample.BlazorWasm.TextEditor.Common.Interop;

namespace Sample.BlazorWasm.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    public sealed class Font : IFont, IAutoFieldDisposable
    {
        private readonly Dictionary<char, CharSize> _measureCache = new Dictionary<char, CharSize>();

        [DisposableField]
        private readonly JSObjectReference _jsFont;

        public JSObjectReference JsObj => _jsFont;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Font(string family, double height, IJSInProcessRuntime jsRuntime)
        {
            _jsFont = JSObjectReference.From(jsRuntime,
                "Sample.BlazorWasm.TextEditor.Font.create", family, height);
        }

        /// <summary>
        /// 
        /// </summary>
        public CharSize MeasureChar(char ch)
        {
            return _measureCache.GetOrAdd(ch, (key, self) => self.InternalMeasureChar(key), this);
        }

        /// <summary>
        /// 
        /// </summary>
        private CharSize InternalMeasureChar(char ch)
        {
            Debug.Assert(CharUtil.CanDraw(ch));

            var size = _jsFont.Call<JsonElement>("measureChar", ch);
            return new CharSize(
                size.GetProperty("width").GetDouble(),
                size.GetProperty("height").GetDouble());
        }
    }
}
