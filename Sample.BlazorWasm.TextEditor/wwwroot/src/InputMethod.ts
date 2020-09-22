namespace Sample.BlazorWasm.TextEditor {
    /**
     * キー入力検出クラス。
     * 日本語入力中の十字キーの左右移動は対応しない（キャレット位置を正確に把握できない為）。
     */
    export class InputMethod {
        /** 日本語入力中かどうか。 */
        private _isJpInputting = false;

        private _isPasting = false;

        private readonly _textarea: HTMLTextAreaElement;
        private readonly _imeDiv: HTMLDivElement;

        private readonly _jsEvent: JSEvent;

        private _font: Font | null = null;

        private _offsetX = 0;
        private _areaWidth = 0;

        /**
         * コンストラクタ。
         */
        public constructor(canvas: HTMLCanvasElement, jsEvent: JSEvent) {
            this._jsEvent = jsEvent;

            this._imeDiv = document.createElement("div");
            canvas.parentElement!.appendChild(this._imeDiv);

            this._textarea = document.createElement("textarea");
            this._textarea.rows = 1;
            this._imeDiv.appendChild(this._textarea);

            this._imeDiv.style.left = "0px";
            this._imeDiv.style.top = "0px";
            this._imeDiv.style.width = "0px";
            this._imeDiv.style.maxWidth = "0px";
            this._imeDiv.style.height = "0px";
            this._imeDiv.style.backgroundColor = "transparent";
            this._imeDiv.style.position = "absolute";
            this._imeDiv.style.overflow = "hidden";
            this._imeDiv.style.zIndex = "-1";
            this._imeDiv.style.margin = "0px";
            this._imeDiv.style.padding = "0px";

            this._textarea.style.left = "0px";
            this._textarea.style.top = "0px";
            this._textarea.style.width = "100px";
            this._textarea.style.resize = "none";
            this._textarea.style.position = "absolute";
            this._textarea.style.outline = "none";
            this._textarea.style.backgroundColor = "white";
            this._textarea.style.border = "none";
            this._textarea.style.margin = "0px";
            this._textarea.style.padding = "0px";

            this._textarea.addEventListener("focusin", () => this.onFocusIn());
            this._textarea.addEventListener("focusout", () => this.onFocusOut());
            this._textarea.addEventListener("compositionupdate", e => this.onCompositionUpdate(e));
            this._textarea.addEventListener("compositionend", () => this.onCompositionEnd());
            this._textarea.addEventListener("keydown", e => this.onKeyDown(e));
            this._textarea.addEventListener("keyup", e => this.onKeyUp(e));
            this._textarea.addEventListener("paste", () => this.onPaste());
        }

        /**
         * 
         */
        private onFocusIn(): void {
            this._jsEvent.raiseAction("InputMethod.focusin", {});
        }

        /**
         * 
         */
        private onFocusOut(): void {
            this._jsEvent.raiseAction("InputMethod.focusout", {});
        }

        /**
         * 日本語入力中
         */
        private onCompositionUpdate(e: Event): void {
            this._isJpInputting = true;

            const oe = e as CompositionEvent;

            this._imeDiv.style.zIndex = "1";

            // 文字列幅
            const width = this._font!.measureTextWidth(oe.data);

            this._textarea.style.width = (width + 100) + "px";
            this._imeDiv.style.width = (this._offsetX/* IME位置 */ + width + 5/* IMEの見た目上での右余白 */) + "px";
            $(this._imeDiv).scrollLeft(0);

            const args: KeyEventArgs = {
                inputKey: "CompositionUpdating",
                isCtrl: false,
                isShift: false,
                isAlt: false,
            };
            if (this._jsEvent.raiseFunc("InputMethod.inputKey", args).handled) {
                e.preventDefault();
                e.stopPropagation();
            }
        }

        /**
         * 日本語入力確定
         */
        private onCompositionEnd(): void {
            this._isJpInputting = false;

            this._jsEvent.raiseAction("InputMethod.inputText", { text: this._textarea.value })
            this._textarea.value = "";

            this._textarea.style.width = "100px";
            $(this._imeDiv).scrollLeft(0);
            this._imeDiv.style.zIndex = "-1";
        }

        /**
         * キーダウン。
         */
        private onKeyDown(e: KeyboardEvent): void {
            if (this._isPasting) {
                e.preventDefault();
                e.stopPropagation();
                return;
            }
            if (this._isJpInputting) {
                return;
            }

            // この後に発生する「貼り付け」イベントで処理する
            if (e.ctrlKey && e.key.toLowerCase() === "v") {
                return;
            }

            this._textarea.value = "";

            // 半角文字の場合
            if (e.key !== "Process") {
                const args: KeyEventArgs = {
                    inputKey: e.key,
                    isCtrl: e.ctrlKey,
                    isShift: e.shiftKey,
                    isAlt: e.altKey,
                };
                if (this._jsEvent.raiseFunc("InputMethod.inputKey", args).handled) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            }
            // 全角スペースの場合
            else if (e.code === "Space") {
                this._jsEvent.raiseAction("InputMethod.inputText", { text: "　" })

                e.preventDefault();
                e.stopPropagation();
            }
        }

        /**
         * キーアップ。
         */
        private onKeyUp(e: KeyboardEvent): void {
            if (this._isPasting) {
                e.preventDefault();
                e.stopPropagation();
                return;
            }
            if (this._isJpInputting) {
                return;
            }

            let handled = false;

            // 半角文字の場合
            if (e.key !== "Process") {
                handled = true;
            }
            // 全角スペースの場合
            else if (e.code === "Space") {
                handled = true;
                this._textarea.value = "";
            }

            if (handled) {
                e.preventDefault();
                e.stopPropagation();
            }
        }

        /**
         * 貼り付けイベント。
         */
        private onPaste(): void {
            if (this._isPasting) {
                return;
            }

            // ここでタイマーを使用する理由
            // ・この時点ではテキストエリアの値にはまだ反映されていない為
            // ・大量のデータをCtrl+Vで貼り付け続けると、その間、再描画されない為
            // ・貼り付けはinputイベントでも検知可能だが、inputイベントはキー入力でも呼ばれるので制御が面倒な為
            this._isPasting = true;
            let retryCount = 0;
            let timerId = -1;
            timerId = window.setInterval(() => {
                const text = this._textarea.value;
                if (text.length > 0 || retryCount++ >= 10) {
                    this._isPasting = false;
                    window.clearInterval(timerId);

                    if (text.length > 0) {
                        this._textarea.value = "";
                        processStringToUTF8(text, (ptr, len) => {
                            this._jsEvent.raiseAction("InputMethod.paste", { ptr, len });
                        });
                    }
                }
            }, 10);
        }

        /**
         * 指定文字列をクリップボードにコピーする。
         * 当該メソッドは「Ctrl+C」or「Ctrl+X」のキーダウンイベントからの同期的処理中のみ呼び出し可能。
         */
        public setClipboardText(ptr: number, len: number): void {
            const text = UTF8ToString(ptr, len);
            if (text.length > 0) {
                this._textarea.value = text;
                this._textarea.select();
                document.execCommand("copy");
                this._textarea.value = "";
            }
        }

        /**
         * フォーカスを設定する。
         */
        public focus(): void {
            this._textarea.focus();
        }

        /**
         * テキスト領域とキャレット位置を設定する。
         */
        public setArea(area: Rect2D, caretLocation: Point2D): void {
            this._offsetX = caretLocation.x - area.x;
            this._areaWidth = area.w;

            this._textarea.style.left = (caretLocation.x - area.x) + "px";
            this._textarea.style.top = (caretLocation.y - area.y) + "px";

            this._imeDiv.style.left = area.x + "px";
            this._imeDiv.style.top = area.y + "px";
            this._imeDiv.style.maxWidth = area.w + "px";
            this._imeDiv.style.height = area.h + "px";

            if (!this._isJpInputting) {
                this._imeDiv.style.width = this._areaWidth + "px";
            }
        }

        /**
         * フォントを設定する。
         */
        public setFont(family: string, height: number, lineHeight: number): void {
            this._font = new Font(family, height);

            this._textarea.style.fontFamily = `'${family}'`;
            this._textarea.style.fontSize = `${height}pt`;
            this._textarea.style.lineHeight = lineHeight + "px";
            this._textarea.style.height = lineHeight + "px";
        }
    }

    /**
     *
     */
    interface KeyEventArgs {
        readonly inputKey: string;
        readonly isCtrl: boolean;
        readonly isShift: boolean;
        readonly isAlt: boolean;
    }
}
