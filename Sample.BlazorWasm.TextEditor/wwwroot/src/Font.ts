namespace Sample.BlazorWasm.TextEditor {
    /**
     * 
     */
    export class Font {
        private readonly _measureCharOffscreen = new Offscreen({ w: 1, h: 1 });

        private readonly _family: string;
        private readonly _height: number;

        public get family() { return this._family; }
        public get height() { return this._height; }

        /**
         * [DotNetInvokable]
         */
        public static create(family: string, height: number): JSObjectId {
            const font = new Font(family, height);
            return JSObjectReference.create(font).id;
        }

        /**
         * 
         */
        public constructor(family: string, height: number) {
            this._family = family;
            this._height = height;

            this._measureCharOffscreen.ctx2d.font = `normal ${this._height}pt '${this._family}'`;
            this._measureCharOffscreen.ctx2d.textBaseline = "top";
        }

        /**
         *
         */
        public measureChar(ch: string): CharSize {
            const width = this._measureCharOffscreen.ctx2d.measureText(ch).width!;
            const heightInPixels = this._height / 72.0 * 96.0;
            return {
                width: width,
                height: heightInPixels,
            };
        }

        /**
         *
         */
        public measureTextWidth(text: string): number {
            return this._measureCharOffscreen.ctx2d.measureText(text).width!;
        }
    }
}
