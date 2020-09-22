using Crash.Core.Drawing;

namespace Crash.Core.UI
{
    public sealed class Settings
    {
        public Colors ui_colors { get; set; } = new Colors();
    }

    public sealed class Colors
    {
        public Color background { get; set; } = Color.FromRgb(0xE8E8EC);

        public Color scrollThumb { get; set; } = Color.FromRgb(0xC2C3C9);

        public Color scrollThumb_active { get; set; } = Color.FromRgb(0x222222);

        public Color scrollThumb_hover { get; set; } = Color.FromRgb(0x222222);

        public Color scrollArrow { get; set; } = Color.FromRgb(0x868999);

        public Color scrollArrow_active { get; set; } = Color.FromRgb(0x222222);

        public Color scrollArrow_hover { get; set; } = Color.FromRgb(0x222222);

        public Color scrollArrow_disable { get; set; } = Color.FromRgb(0xC2C3C9);
    }
}
