namespace Sample.BlazorWasm.TextEditor {
    /**
     * 
     */
    export class Offscreen {
        public readonly canvas: HTMLCanvasElement;
        public readonly ctx2d: CanvasRenderingContext2D;

        /**
         * [DotNetInvokable]
         */
        public static create(size: Size2D): JSObjectId {
            const offscreen = new Offscreen(size);
            return JSObjectReference.create(offscreen).id;
        }

        /**
         * 
         */
        public constructor(size: Size2D) {
            this.canvas = document.createElement("canvas") as HTMLCanvasElement;
            this.ctx2d = this.canvas.getContext("2d", { alpha: false })!;

            this.setSize(size.w, size.h);
        }

        /**
         * 
         */
        public resize(size: Size2D): void {
            this.setSize(size.w, size.h);
        }

        /**
         * 
         */
        private setSize(width: number, height: number): void {
            this.canvas.width = Math.max(width, 1);
            this.canvas.height = Math.max(height, 1);
        }

        /**
         *
         */
        public callCommands(ptrUtf8Json: number, length: number): void {
            const json = UTF8ToString(ptrUtf8Json, length);
            const parameter = JSON.parse(json);
            for (const func of parameter.funcs) {
                (this as any)[func.name](func.arg);
            }
        }

        /**
         *
         */
        private setColor(color: string): void {
            this.ctx2d.fillStyle = color;
        }

        /**
         *
         */
        private setFont(fontObjId: JSObjectId): void {
            const font = JSObjectReference.from<Font>(fontObjId).obj;

            this.ctx2d.font = `normal ${font.height}pt '${font.family}'`;
            this.ctx2d.textBaseline = "top";
        }

        /**
         *
         */
        private save(): void {
            this.ctx2d.save();
        }

        /**
         *
         */
        private restore(): void {
            this.ctx2d.restore();
        }

        /**
         *
         */
        private clip(clipRects: Rect2D[]): void {
            const region = new Path2D();
            for (let rect of clipRects) {
                region.rect(rect.x, rect.y, rect.w, rect.h);
            }
            this.ctx2d.clip(region);
        }

        /**
         *
         */
        private drawString(arg: { str: string, pt: Point2D }): void {
            this.ctx2d.fillText(arg.str, arg.pt.x, arg.pt.y);
        }

        /**
         * 
         */
        private fillRect(rect: Rect2D): void {
            this.ctx2d.fillRect(rect.x, rect.y, rect.w, rect.h);
        }

        /**
         * 
         */
        private fillPolygon(points: Point2D[]): void {
            this.ctx2d.beginPath();
            this.ctx2d.moveTo(points[0].x, points[0].y);
            const length = points.length;
            for (var i = 1; i < length; i++) {
                this.ctx2d.lineTo(points[i].x, points[i].y);
            }
            this.ctx2d.fill();
        }

        /**
         *
         */
        private drawOffscreen(arg: { id: JSObjectId, pt: Point2D }): void {
            const offscreen = JSObjectReference.from<Offscreen>(arg.id).obj;
            this.ctx2d.drawImage(offscreen.canvas, arg.pt.x, arg.pt.y);
        }
    }
}
