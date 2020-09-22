using System;
using System.Collections.Generic;
using System.Text.Json;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.Text;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.Presenter;
using Microsoft.JSInterop;
using Sample.BlazorWasm.TextEditor.Common.Interop;
using Sample.BlazorWasm.TextEditor.UIContextImpl;

namespace Sample.BlazorWasm.TextEditor.Shared
{
    /// <summary>
    /// 
    /// </summary>
    partial class TextEditorComponent : IDisposable
    {
        private readonly Disposer _disposer = new Disposer();

        private TextPresenter _presenter = null!;

        private Dictionary<(JSKey, ModifierKeys), Action> _keyBindings = null!;

        private IJSInProcessRuntime JsInProcessRuntime => (IJSInProcessRuntime)JSRuntime;

        private JSObjectReference _jsRefTextEditorComponent = null!;

        private InputMethod _inputMethod = null!;

        internal IInputMethod InputMethod => _inputMethod;

        public event Action<Size2D>? Created;

        public event Action<JSObjectReference>? CreatedComponent;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _jsRefTextEditorComponent.CallVoid("destroy");
            _disposer.Dispose();
        }

        protected override bool ShouldRender() => false;

        /// <summary>
        /// 
        /// </summary>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            var jsEvent = new JSEvent();

            var dotNetRefEvent = DotNetObjectReference.Create(jsEvent);

            _jsRefTextEditorComponent = JSObjectReference.From(JsInProcessRuntime,
                "Sample.BlazorWasm.TextEditor.TextEditorComponent.create", Id, dotNetRefEvent);

            _inputMethod = new InputMethod(_jsRefTextEditorComponent);

            _presenter = new TextPresenter(new CanvasContext(this, JsInProcessRuntime, jsEvent), @"
{
    ""ui_colors"": {
        ""background"": ""#E8E8EC"",
        ""scrollThumb"": ""#C2C3C9"",
        ""scrollThumb_active"": ""#222222"",
        ""scrollArrow"": ""#868999"",
        ""scrollArrow_active"": ""#222222"",
        ""scrollArrow_disable"": ""#C2C3C9"",
    },
    ""fonts"": {
        ""asciiFont"": { ""name"": ""Consolas"", ""height"": 14.0 },
        ""jpFont"": { ""name"": ""メイリオ"", ""height"": 12.0 },
    },
    ""tabWidth"": 4,
    ""scrollSpeed_vertical"": 3,
    ""scrollSpeed_horizontal"": 4,
    ""lineHeightAdjust"": 3,
}");

            _keyBindings = BindKeys();

            jsEvent.RegisterActionListener("InputMethod.inputText", OnInputText);
            jsEvent.RegisterFuncListener("InputMethod.inputKey", OnInputKey);
            jsEvent.RegisterActionListener("InputMethod.paste", OnPaste);

            _disposer.Add(dotNetRefEvent);
            _disposer.Add(_jsRefTextEditorComponent);
            _disposer.Add(InputMethod);
            _disposer.Add(_presenter);

            Created?.Invoke(new Size2D(Width, Height));
            CreatedComponent?.Invoke(_jsRefTextEditorComponent);
        }

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<(JSKey, ModifierKeys), Action> BindKeys()
        {
            return new Dictionary<(JSKey, ModifierKeys), Action>
            {
                // カーソル移動系
                [(JSKey.ArrowUp, ModifierKeys.None)] = () => _presenter.Up(),
                [(JSKey.ArrowDown, ModifierKeys.None)] = () => _presenter.Down(),
                [(JSKey.ArrowLeft, ModifierKeys.None)] = () => _presenter.Left(),
                [(JSKey.ArrowRight, ModifierKeys.None)] = () => _presenter.Right(),
                [(JSKey.Home, ModifierKeys.None)] = () => _presenter.MoveLineTop(),
                [(JSKey.Home, ModifierKeys.Ctrl)] = () => _presenter.MoveTop(),
                [(JSKey.End, ModifierKeys.None)] = () => _presenter.MoveLineEnd(),
                [(JSKey.End, ModifierKeys.Ctrl)] = () => _presenter.MoveEnd(),
                [(JSKey.PageUp, ModifierKeys.None)] = () => _presenter.PageUp(),
                [(JSKey.PageDown, ModifierKeys.None)] = () => _presenter.PageDown(),
                [(JSKey.ArrowRight, ModifierKeys.Ctrl)] = () => _presenter.MoveNextWord(),
                [(JSKey.ArrowLeft, ModifierKeys.Ctrl)] = () => _presenter.MovePrevWord(),

                // カーソル移動系（選択）
                [(JSKey.ArrowUp, ModifierKeys.Shift)] = () => _presenter.UpSelect(),
                [(JSKey.ArrowDown, ModifierKeys.Shift)] = () => _presenter.DownSelect(),
                [(JSKey.ArrowLeft, ModifierKeys.Shift)] = () => _presenter.LeftSelect(),
                [(JSKey.ArrowRight, ModifierKeys.Shift)] = () => _presenter.RightSelect(),
                [(JSKey.Home, ModifierKeys.Shift)] = () => _presenter.MoveLineTopSelect(),
                [(JSKey.Home, ModifierKeys.Ctrl | ModifierKeys.Shift)] = () => _presenter.MoveTopSelect(),
                [(JSKey.End, ModifierKeys.Shift)] = () => _presenter.MoveLineEndSelect(),
                [(JSKey.End, ModifierKeys.Ctrl | ModifierKeys.Shift)] = () => _presenter.MoveEndSelect(),
                [(JSKey.PageUp, ModifierKeys.Shift)] = () => _presenter.PageUpSelect(),
                [(JSKey.PageDown, ModifierKeys.Shift)] = () => _presenter.PageDownSelect(),
                [(JSKey.ArrowRight, ModifierKeys.Ctrl | ModifierKeys.Shift)] = () => _presenter.MoveNextWordSelect(),
                [(JSKey.ArrowLeft, ModifierKeys.Ctrl | ModifierKeys.Shift)] = () => _presenter.MovePrevWordSelect(),

                // 編集系
                [(JSKey.Z, ModifierKeys.Ctrl)] = () => _presenter.Undo(),
                [(JSKey.Y, ModifierKeys.Ctrl)] = () => _presenter.Redo(),
                [(JSKey.Enter, ModifierKeys.None)] = () => _presenter.NewLine(),
                [(JSKey.Delete, ModifierKeys.None)] = () => _presenter.Delete(),
                [(JSKey.Backspace, ModifierKeys.None)] = () => _presenter.DeleteBack(),
                [(JSKey.Tab, ModifierKeys.None)] = () => _presenter.Indent(),
                [(JSKey.Tab, ModifierKeys.Shift)] = () => _presenter.Unindent(),

                // クリップボード系
                [(JSKey.C, ModifierKeys.Ctrl)] = () => Copy(),
                [(JSKey.X, ModifierKeys.Ctrl)] = () => Cut(),

                // 選択系
                [(JSKey.A, ModifierKeys.Ctrl)] = () => _presenter.SelectAll(),
                [(JSKey.Escape, ModifierKeys.None)] = () => _presenter.ClearSelect(),

                // スクロール系
                [(JSKey.ArrowUp, ModifierKeys.Ctrl)] = () => _presenter.ScrollUp(),
                [(JSKey.ArrowDown, ModifierKeys.Ctrl)] = () => _presenter.ScrollDown(),
                [(JSKey.CompositionUpdating, ModifierKeys.None)] = () => _presenter.ScrollCaret(),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private enum JSKey
        {
            ArrowUp = 10000, // Enum.TryParseは「フィールド名 or 値」の文字列に一致する場合に変換するので、数字キー押下を考慮して大きい値にしておく
            ArrowDown,
            ArrowLeft,
            ArrowRight,
            Tab,
            Enter,
            PageUp,
            PageDown,
            Home,
            End,
            Delete,
            Backspace,
            Escape,
            Space,
            A,
            Z,
            Y,
            C,
            X,
            CompositionUpdating, // IME入力中
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnInputText(JsonElement args)
        {
            _presenter.InsertText(args.GetProperty("text").GetString());
        }

        /// <summary>
        /// 
        /// </summary>
        private object OnInputKey(JsonElement args)
        {
            var handled = false;
            var inputKey = args.GetProperty("inputKey").GetString();
            var modifierKeys = ModifierKeysConverter.ToModifierKeys(
                args.GetProperty("isCtrl").GetBoolean(),
                args.GetProperty("isShift").GetBoolean(),
                args.GetProperty("isAlt").GetBoolean());
            if (Enum.TryParse<JSKey>(inputKey, true, out var jsKey)
                && _keyBindings.TryGetValue((jsKey, modifierKeys), out var bindedAction))
            {
                bindedAction();
                handled = true;
            }
            else if (inputKey.Length == 1 && CharUtil.IsAscii(inputKey[0]))
            {
                _presenter.InsertText(inputKey);
                handled = true;
            }

            return new { handled };
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnPaste(JsonElement args)
        {
            string str;
            unsafe
            {
                var ptr = (byte*)args.GetProperty("ptr").GetUInt64();
                var len = args.GetProperty("len").GetInt32();
                str = TextEncodes.UTF8NoBOM.GetString(ptr, len);
            }
            _presenter.InsertText(str);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Copy()
        {
            var text = _presenter.GetSelectionText();
            _inputMethod.SetClipboardText(text);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Cut()
        {
            Copy();
            _presenter.RemoveSelectionText();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void RequestRender()
        {
            _jsRefTextEditorComponent.CallVoid("requestRender");
        }

        /// <summary>
        /// 
        /// </summary>
        internal void SetCursor(JSCursor cursor)
        {
            _jsRefTextEditorComponent.CallVoid("setCursor", cursor.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        internal void TransferScreen(JSObjectReference jsObjOffscreen)
        {
            _jsRefTextEditorComponent.CallVoid("transferScreen", jsObjOffscreen.Id);
        }
    }

    internal enum JSCursor
    {
        @default,
        text,
    }
}
