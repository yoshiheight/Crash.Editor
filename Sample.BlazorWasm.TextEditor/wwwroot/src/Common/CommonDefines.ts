namespace Sample.BlazorWasm.TextEditor {
    export interface Point2D {
        readonly x: number;
        readonly y: number;
    }

    export interface Size2D {
        readonly w: number;
        readonly h: number;
    }

    export interface Rect2D {
        readonly x: number;
        readonly y: number;
        readonly w: number;
        readonly h: number;
    }

    export interface CharSize {
        readonly width: number;
        readonly height: number;
    }
}
