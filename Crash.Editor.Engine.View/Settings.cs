namespace Crash.Editor.Engine.View
{
    public sealed class Settings
    {
        public int tabWidth { get; set; } = 4;

        public int scrollSpeed_vertical { get; set; } = 3;

        public int scrollSpeed_horizontal { get; set; } = 4;

        public int lineHeightAdjust { get; set; } = 0;

        public Fonts fonts { get; set; } = null!;
    }

    public sealed class Fonts
    {
        public FontInfo asciiFont { get; set; } = null!;

        public FontInfo jpFont { get; set; } = null!;
    }

    public sealed class FontInfo
    {
        public string name { get; set; } = null!;

        public double height { get; set; }
    }
}
