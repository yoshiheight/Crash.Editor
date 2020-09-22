namespace Sample.BlazorWasm.TextEditor {
    export type JSObjectId = number;

    /**
     * JavaScriptオブジェクト参照インターフェイス。
     */
    interface IJSObjectReference {
        readonly id: JSObjectId;
        readonly obj: object;
        dispose(): void;
    }

    /**
     * JavaScriptオブジェクト参照。
     */
    export class JSObjectReference<TObject extends object> implements IJSObjectReference {
        private static readonly __map = new Map<JSObjectId, IJSObjectReference>();

        private static __seq: JSObjectId = 1;

        private readonly _id: JSObjectId;

        private readonly _obj: TObject;

        public get id(): JSObjectId { return this._id; }

        public get obj(): TObject { return this._obj; }

        /**
         * JavaScriptオブジェクト参照を新規作成。
         */
        public static create<TObject extends object>(obj: TObject): JSObjectReference<TObject> {
            return new JSObjectReference<TObject>(obj);
        }

        /**
         * 既存のJavaScriptオブジェクト参照を取得。
         */
        public static from<TObject extends object>(id: JSObjectId): JSObjectReference<TObject> {
            return JSObjectReference.__map.get(id) as JSObjectReference<TObject>;
        }

        /**
         * コンストラクタ。
         */
        private constructor(obj: TObject) {
            this._id = JSObjectReference.__seq++;
            this._obj = obj;
            JSObjectReference.__map.set(this._id, this);
        }

        /**
         * 破棄処理。
         * JavaScript側から破棄する場合に使用する。
         */
        public dispose(): void {
            JSObjectReference.__map.delete(this._id);
        }

        /**
         * [DotNetInvokable]
         * 破棄処理。
         * DotNet側から破棄する場合に使用する。
         */
        public static dispose(id: JSObjectId): void {
            JSObjectReference.from(id).dispose();
        }

        /**
         * [DotNetInvokable]
         * 参照先オブジェクトのインスタンスメソッド呼び出し。
         */
        public static call(id: JSObjectId, methodName: string, args: any[]): any {
            return JSObjectReference.internalCall(id, methodName, args);
        }

        /**
         * [DotNetInvokable]
         * 参照先オブジェクトのインスタンスメソッド呼び出し。
         */
        public static callVoid(id: JSObjectId, methodName: string, args: any[]): void {
            JSObjectReference.internalCall(id, methodName, args);
        }

        /**
         * 参照先オブジェクトのインスタンスメソッド呼び出し。
         */
        private static internalCall(id: JSObjectId, methodName: string, args: any[]): any {
            const obj = JSObjectReference.__map.get(id)!.obj as any;
            return obj[methodName].apply(obj, args);
        }
    }
}
