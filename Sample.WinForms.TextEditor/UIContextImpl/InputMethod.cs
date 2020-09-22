using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI.UIContext;

namespace Sample.WinForms.TextEditor.UIContextImpl
{
    /// <summary>
    /// IME機能を提供する。
    /// IMEのオン/オフ切り替えは Control.CanEnableIme をオーバーライドすることで自動的に機能する。
    /// またキーボード入力された文字はIMEのオン/オフの状態問わず Control.KeyPress イベントで検出可能である。
    /// その為、当クラスで提供する機能はIMEの表示位置設定、表示範囲設定、フォント設定のみである。
    /// </summary>
    public sealed class InputMethod : IInputMethod, IAutoFieldDisposable
    {
        [Aggregation]
        private readonly IntPtr _hWnd;

        [Aggregation]
        private readonly Func<bool> _Focused;

        private Point2D _location;
        private Rect2D _area;
        private string _fontName;
        private double _height;

        public InputMethod(IntPtr hWnd, Func<bool> focused)
        {
            _hWnd = hWnd;
            _Focused = focused;
        }

        public void SetArea(Rect2D rect, Point2D location)
        {
            if (rect.IsInvalid)
            {
                return;
            }

            location = location.Clamp(rect);

#warning 相違がある時だけ行う
            if (true)
            {
                _location = location;
                _area = rect;

                Reset();
            }
        }

        public void SetFont(string fontName, double fontHeight, int lineHeight)
        {
#warning 相違がある時だけ行う
            _fontName = fontName;
            _height = fontHeight;

            Reset();
        }

        private void Reset()
        {
            if (!_Focused())
            {
                return;
            }

            var hImc = Imm32Api.GetContext(_hWnd);
            try
            {
                Imm32Api.SetCompositionWindow(hImc, _location, _area);
                Imm32Api.SetCompositionFont(hImc, _fontName, _height);
            }
            finally
            {
                Imm32Api.ReleaseContext(_hWnd, hImc);
            }
        }

        /// <summary>
        /// IMM32APIを提供する。
        /// </summary>
        private static class Imm32Api
        {
            public static IntPtr GetContext(IntPtr hwnd)
            {
                var hImc = NativeApi.ImmGetContext(hwnd);
                Verifier.Verify<Win32Exception>(hImc != null);
                return hImc;
            }

            public static void ReleaseContext(IntPtr hwnd, IntPtr hImc)
            {
                var result = NativeApi.ImmReleaseContext(hwnd, hImc);
                Verifier.Verify<Win32Exception>(result);
            }

            public static void SetCompositionWindow(IntPtr hImc, Point2D pos, Rect2D rect)
            {
                NativeApi.COMPOSITIONFORM cf;
                cf.dwStyle = NativeApi.CFS_POINT | NativeApi.CFS_RECT;
                cf.ptCurrentPos.x = pos.X;
                cf.ptCurrentPos.y = pos.Y;
                cf.rcArea.left = rect.Left;
                cf.rcArea.top = rect.Top;
                cf.rcArea.right = rect.Right;
                cf.rcArea.bottom = rect.Bottom;

                var result = NativeApi.ImmSetCompositionWindow(hImc, ref cf);
                Verifier.Verify<Win32Exception>(result);
            }

            public static void SetCompositionFont(IntPtr hImc, string fontName, double heightInPoints)
            {
                var heightInPixels = Convert.ToInt32(heightInPoints / 72.0 * 96.0);

                var logFont = new NativeApi.LOGFONT();
                logFont.lfCharSet = 128;
                logFont.lfFaceName = fontName;
                logFont.lfHeight = Convert.ToInt32(heightInPixels) * -1;
                logFont.lfWeight = 400;

                var result = NativeApi.ImmSetCompositionFont(hImc, ref logFont);
                Verifier.Verify<Win32Exception>(result);
            }

            /// <summary>
            /// IMM32のネイティブAPI定義。
            /// </summary>
            private static class NativeApi
            {
                [DllImport("Imm32.dll")]
                public static extern IntPtr ImmGetContext(IntPtr hWnd);

                [DllImport("Imm32.dll")]
                public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

                [DllImport("imm32.dll")]
                public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompositionForm);

                [DllImport("imm32.dll", CharSet = CharSet.Auto)]
                public static extern bool ImmSetCompositionFont(IntPtr hIMC, ref LOGFONT lplf);

                public const uint CFS_RECT = 0x0001;
                public const uint CFS_POINT = 0x0002;

                [StructLayout(LayoutKind.Sequential)]
                public struct POINT
                {
                    public int x;
                    public int y;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct RECT
                {
                    public int left;
                    public int top;
                    public int right;
                    public int bottom;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct COMPOSITIONFORM
                {
                    public uint dwStyle;
                    public POINT ptCurrentPos;
                    public RECT rcArea;
                }

                [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
                public struct LOGFONT
                {
                    public const int LF_FACESIZE = 32;
                    public int lfHeight;
                    public int lfWidth;
                    public int lfEscapement;
                    public int lfOrientation;
                    public int lfWeight;
                    public byte lfItalic;
                    public byte lfUnderline;
                    public byte lfStrikeOut;
                    public byte lfCharSet;
                    public byte lfOutPrecision;
                    public byte lfClipPrecision;
                    public byte lfQuality;
                    public byte lfPitchAndFamily;
                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
                    public string? lfFaceName;
                }
            }
        }
    }
}
