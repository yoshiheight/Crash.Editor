namespace Sample.BlazorWasm.TextEditor {
    /**
     * クラス。
     */
    export class TextEditorComponent {
        private readonly _visualCanvas: HTMLCanvasElement;
        private readonly _visualCtx2d: CanvasRenderingContext2D;

        private readonly _jsEvent: JSEvent;
        private readonly _inputMethod: InputMethod;

        private _destroyer = () => { };

        public get size(): Size2D { return { w: this._visualCanvas.width, h: this._visualCanvas.height }; }

        /**
         * [DotNetInvokable]
         */
        public static create(canvasElementId: string, objRefJsec: DotNetObjectReference): JSObjectId {
            const jsEvent = new JSEvent(objRefJsec);
            const textEditorComponent = new TextEditorComponent(canvasElementId, jsEvent);
            return JSObjectReference.create(textEditorComponent).id;
        }

        /**
         * コンストラクタ。
         */
        public constructor(canvasElementId: string, jsEvent: JSEvent) {
            this._visualCanvas = document.getElementById(canvasElementId) as HTMLCanvasElement;
            this._visualCtx2d = this._visualCanvas.getContext("2d", { alpha: false })!;
            this._jsEvent = jsEvent;
            this._inputMethod = new InputMethod(this._visualCanvas, jsEvent);

            this.initEventListener();
        }

        /**
         * 後処理。
         */
        public destroy(): void {
            this._destroyer();
        }

        /**
         * 
         */
        private initEventListener(): void {
            const callbackMouseDown = (e: MouseEvent) => this.onMouseDown(e);
            const callbackMouseUp = (e: MouseEvent) => this.onMouseUp(e);
            const callbackMouseMove = (e: MouseEvent) => this.onMouseMove(e);

            document.addEventListener("mousedown", callbackMouseDown);
            document.addEventListener("mouseup", callbackMouseUp);
            document.addEventListener("mousemove", callbackMouseMove);

            this._destroyer = () => {
                document.removeEventListener("mousedown", callbackMouseDown);
                document.removeEventListener("mouseup", callbackMouseUp);
                document.removeEventListener("mousemove", callbackMouseMove);
            };

            this._visualCanvas.addEventListener("wheel", e => this.onMouseWheel(e))

            new MutationObserver(() => this.onResize())
                .observe(this._visualCanvas, {
                    attributes: true,
                    attributeFilter: ["width", "height"],
                });
        }

        /**
         *
         */
        private onMouseDown(e: MouseEvent): void {
            this._inputMethod.focus();
            this._jsEvent.raiseAction("TextEditorComponent.mousedown", this.toMouseEventArgs(e));

            // 対象がキャンバスの時だけfalseにする!!!!!!!
            e.preventDefault();
            e.stopPropagation();
        }

        /**
         *
         */
        private onMouseUp(e: MouseEvent): void {
            this._jsEvent.raiseAction("TextEditorComponent.mouseup", this.toMouseEventArgs(e));

            // 対象がキャンバスの時だけfalseにする!!!!!!!
            e.preventDefault();
            e.stopPropagation();
        }

        /**
         *
         */
        private onMouseMove(e: MouseEvent): void {
            this._jsEvent.raiseAction("TextEditorComponent.mousemove", this.toMouseEventArgs(e));

            // 対象がキャンバスの時だけfalseにする!!!!!!!
            e.preventDefault();
            e.stopPropagation();
        }

        /**
         *
         */
        private onMouseWheel(e: WheelEvent): void {
            this._jsEvent.raiseAction("TextEditorComponent.mousewheel", this.toMouseEventArgs(e));
            e.preventDefault();
            e.stopPropagation();
        }

        /**
         *
         */
        private onResize(): void {
            this._jsEvent.raiseAction("TextEditorComponent.resize", this.size);
        }

        /**
         * 
         */
        public bindInputMethod(): JSObjectId {
            return JSObjectReference.create(this._inputMethod).id;
        }

        /**
         * 
         */
        public focus(): void {
            this._inputMethod.focus();
        }

        /**
         * 
         */
        public resize(width: number, height: number): void {
            this._visualCanvas.width = width;
            this._visualCanvas.height = height;
        }

        /**
         * 
         */
        public requestRender(): void {
            window.requestAnimationFrame(timestamp => {
                this._jsEvent.raiseAction("TextEditorComponent.render", {});
            });
        }

        /**
         * setCursor
         */
        public setCursor(cursor: string): void {
            this._visualCanvas.style.cursor = cursor;
        }

        /**
         *
         */
        public transferScreen(offscreenId: JSObjectId): void {
            const offscreen = JSObjectReference.from<Offscreen>(offscreenId).obj;
            this._visualCtx2d.drawImage(offscreen.canvas, 0, 0);
        }

        /**
         *
         */
        private toMouseEventArgs(e: MouseEvent): MouseEventArgs {
            let notchY = 0;
            if (e instanceof WheelEvent) {
                const notchDelta = (e.deltaMode == WheelEvent.DOM_DELTA_PIXEL) ? 100.0
                    : (e.deltaMode == WheelEvent.DOM_DELTA_LINE) ? 3.0
                        : 1.0;
                notchY = Math.ceil(Math.abs(e.deltaY) / notchDelta) * Math.sign(e.deltaY) * -1;
            }

            const offset = $(this._visualCanvas).offset()!;

            return {
                x: Math.ceil(e.pageX - offset.left),
                y: Math.ceil(e.pageY - offset.top),
                notchY: notchY,
                button: e.button,
                isCtrl: e.ctrlKey,
                isShift: e.shiftKey,
                isAlt: e.altKey,
            };
        }
    }

    /**
     * 
     */
    interface MouseEventArgs {
        readonly x: number;
        readonly y: number;
        readonly notchY: number;
        readonly button: number;
        readonly isCtrl: boolean;
        readonly isShift: boolean;
        readonly isAlt: boolean;
    }

    //declare var BINDING: {
    //    conv_string(dotNetString: string): string;
    //};
}
