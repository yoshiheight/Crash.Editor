namespace Sample.BlazorWasm.TextEditor {
    /**
     * 
     */
    export class JSEvent {
        /**
         *
         */
        public constructor(
            private readonly _objRef: DotNetObjectReference) {
        }

        /**
         *
         */
        public raiseAction(eventName: string, arg: any): void {
            this._objRef.invokeMethod("CallbackAction", eventName, arg);
        }

        /**
         *
         */
        public raiseFunc(eventName: string, arg: any): any {
            return this._objRef.invokeMethod("CallbackFunc", eventName, arg);
        }
    }
}
