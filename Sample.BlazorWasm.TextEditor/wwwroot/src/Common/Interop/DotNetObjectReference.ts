namespace Sample.BlazorWasm.TextEditor {
    /**
     * .NETオブジェクト参照。
     */
    export interface DotNetObjectReference {
        /**
         * インスタンスメソッドを呼び出す。
         */
        invokeMethod(methodName: string, ...args: any[]): any;
    }
}
