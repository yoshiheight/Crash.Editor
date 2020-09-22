using System;
using System.Collections.Generic;
using System.Text.Json;
using Crash.Core.Collections;
using Microsoft.JSInterop;

namespace Sample.BlazorWasm.TextEditor.Common.Interop
{
    public sealed class JSEvent
    {
        private readonly Dictionary<string, Delegate> _listenerMap = new Dictionary<string, Delegate>();

        public void RegisterActionListener(string eventName, Action<JsonElement> callback)
        {
            _listenerMap.Add(eventName, callback);
        }

        public void RegisterFuncListener(string eventName, Func<JsonElement, object> callback)
        {
            _listenerMap.Add(eventName, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        [JSInvokable]
        public void CallbackAction(string eventName, JsonElement jsonElem)
        {
            _listenerMap.GetValueOrNull(eventName)?.DynamicInvoke(jsonElem);
        }

        /// <summary>
        /// 
        /// </summary>
        [JSInvokable]
        public object? CallbackFunc(string eventName, JsonElement jsonElem)
        {
            return _listenerMap.GetValueOrNull(eventName)?.DynamicInvoke(jsonElem);
        }
    }
}
