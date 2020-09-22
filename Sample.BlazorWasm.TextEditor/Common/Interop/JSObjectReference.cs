using System;
using Crash.Core;
using Microsoft.JSInterop;

namespace Sample.BlazorWasm.TextEditor.Common.Interop
{
    using JSObjectId = Int32;

    /// <summary>
    /// 
    /// </summary>
    public sealed class JSObjectReference : IDisposable
    {
        [Aggregation]
        private readonly IJSInProcessRuntime _jsRuntime;

        public JSObjectId Id { get; }

        /// <summary>
        /// 
        /// </summary>
        private JSObjectReference(JSObjectId id, IJSInProcessRuntime jsRuntime)
        {
            Id = id;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// 
        /// </summary>
        public static JSObjectReference From(IJSInProcessRuntime jsRuntime, string identifier, params object[] args)
        {
            var id = jsRuntime.Invoke<JSObjectId>(identifier, args);
            return new JSObjectReference(id, jsRuntime);
        }

        /// <summary>
        /// 
        /// </summary>
        public static JSObjectReference From(JSObjectReference jsObj, string identifier, params object[] args)
        {
            return new JSObjectReference(jsObj.Call<JSObjectId>(identifier, args), jsObj._jsRuntime);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _jsRuntime.InvokeVoid("Sample.BlazorWasm.TextEditor.JSObjectReference.dispose", Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public T Call<T>(string methodName, params object[] args)
        {
            return _jsRuntime.Invoke<T>("Sample.BlazorWasm.TextEditor.JSObjectReference.call", Id, methodName, args);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CallVoid(string methodName, params object[] args)
        {
            _jsRuntime.InvokeVoid("Sample.BlazorWasm.TextEditor.JSObjectReference.callVoid", Id, methodName, args);
        }
    }
}
