type Ptr = number;

declare var Module: Sample.BlazorWasm.TextEditor.Module;
declare function lengthBytesUTF8(str: string): number;
declare function stringToUTF8(str: string, destPtr: Ptr, maxBytesToWrite: number): void;
declare function UTF8ToString(srcPtr: Ptr, maxBytesToRead?: number): string;

namespace Sample.BlazorWasm.TextEditor {
    export interface Module {
        _malloc(size: number): Ptr;
        _free(ptr: Ptr): void;

        readonly HEAPU8: HEAPU8;
    }

    export interface HEAPU8 {
        readonly buffer: ArrayBuffer;
    }

    export function processStringToUTF8(str: string, callback: (ptr: Ptr, bytesExclusiveNull: number) => void): void {
        const bufferSize = lengthBytesUTF8(str) + 1;
        const ptr = Module._malloc(bufferSize);
        try {
            stringToUTF8(str, ptr, bufferSize);
            //var arrayView = new Uint8Array(Module.HEAPU8.buffer, ptr, bufferSize);
            //arrayView[bufferSize - 1] = 0;

            callback(ptr, bufferSize - 1);
        }
        finally {
            Module._free(ptr);
        }
    }
}
