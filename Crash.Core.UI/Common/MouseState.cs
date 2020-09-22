using System;
using Crash.Core.Drawing;

namespace Crash.Core.UI.Common
{
    [Immutable]
    public sealed class MouseState
    {
        public Point2D Location { get; }

        public int WheelNotchY { get; }

        public MouseButtons Buttons { get; }

        public bool IsLeft => Buttons.HasFlag(MouseButtons.Left);

        public bool IsRight => Buttons.HasFlag(MouseButtons.Right);

        public bool IsMiddle => Buttons.HasFlag(MouseButtons.Middle);

        public ModifierKeys ModifierKeys { get; }

        public bool IsCtrl => ModifierKeys.HasFlag(ModifierKeys.Ctrl);

        public bool IsShift => ModifierKeys.HasFlag(ModifierKeys.Shift);

        public bool IsAlt => ModifierKeys.HasFlag(ModifierKeys.Alt);

        public bool IsModifierKey => ModifierKeys != ModifierKeys.None;

        public MouseState(int x, int y, int wheelNotchY,
            bool isLeft, bool isRight, bool isMiddle,
            bool isCtrl, bool isShift, bool isAlt)
        {
            Location = new Point2D(x, y);
            WheelNotchY = wheelNotchY;
            Buttons = MouseButtonsConverter.ToMouseButtons(isLeft, isRight, isMiddle);
            ModifierKeys = ModifierKeysConverter.ToModifierKeys(isCtrl, isShift, isAlt);
        }
    }

    /// <summary>
    /// マウスボタン。
    /// </summary>
    [Flags]
    public enum MouseButtons
    {
        None,
        Left = 0x01,
        Right = 0x02,
        Middle = 0x04,
    }

    public static class MouseButtonsConverter
    {
        public static MouseButtons ToMouseButtons(bool isLeft, bool isRight, bool isMiddle)
        {
            return (isLeft ? MouseButtons.Left : MouseButtons.None)
                | (isRight ? MouseButtons.Right : MouseButtons.None)
                | (isMiddle ? MouseButtons.Middle : MouseButtons.None);
        }
    }

    /// <summary>
    /// 修飾キー。
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        None,
        Ctrl = 0x01,
        Shift = 0x02,
        Alt = 0x04,
    }

    public static class ModifierKeysConverter
    {
        public static ModifierKeys ToModifierKeys(bool isCtrl, bool isShift, bool isAlt)
        {
            return (isCtrl ? ModifierKeys.Ctrl : ModifierKeys.None)
                | (isShift ? ModifierKeys.Shift : ModifierKeys.None)
                | (isAlt ? ModifierKeys.Alt : ModifierKeys.None);
        }
    }
}
