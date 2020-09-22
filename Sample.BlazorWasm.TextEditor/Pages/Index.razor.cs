using Microsoft.JSInterop;
using Sample.BlazorWasm.TextEditor.Shared;

namespace Sample.BlazorWasm.TextEditor.Pages
{
    partial class Index
    {
        private TextEditorComponent _myTextEditor = null!;

        private IJSInProcessRuntime JsInProcessRuntime => (IJSInProcessRuntime)JSRuntime;

        protected override bool ShouldRender() => false;

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            _myTextEditor.CreatedComponent += jsObj =>
            {
                JsInProcessRuntime.InvokeVoid("Sample_BlazorWasm_TextEditor.initTextEditorLayout", jsObj.Id);
            };
        }
    }
}
