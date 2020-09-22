using System.Collections.Generic;
using System.Diagnostics;
using Crash.Core;
using Crash.Core.Collections;
using Crash.Core.Drawing;
using Crash.Core.Text;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;

namespace Sample.WinForms.TextEditor.UIContextImpl
{
    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    public sealed class Font : IFont, IAutoFieldDisposable
    {
        private readonly Dictionary<char, CharSize> _measureCache = new Dictionary<char, CharSize>();

        [DisposableField]
        private readonly System.Drawing.Font _font;

        [DisposableField]
        private readonly Offscreen _measureCharOffscreen = new Offscreen(new Size2D(1, 1));

        public System.Drawing.Font Value => _font;

        public Font(string family, double height)
        {
            _font = new System.Drawing.Font(family, (float)height, System.Drawing.GraphicsUnit.Point);
        }

        public CharSize MeasureChar(char ch)
        {
            return _measureCache.GetOrAdd(ch, (key, self) => self.InternalMeasureChar(key), this);
        }

        private CharSize InternalMeasureChar(char ch)
        {
            Debug.Assert(CharUtil.CanDraw(ch));

            using (var sf = new System.Drawing.StringFormat(System.Drawing.StringFormat.GenericTypographic))
            {
                var g = _measureCharOffscreen.Graphics;
                sf.SetMeasurableCharacterRanges(new[] { new System.Drawing.CharacterRange(0, 1) });
                var text = ch + "a"; // 文字がスペースの場合にサイズが0になってしまうので、その回避
                var regions = g.MeasureCharacterRanges(text, _font, new System.Drawing.RectangleF(0, 0, 1000, 1000), sf);
                System.Drawing.RectangleF rect = regions[0].GetBounds(g);
                return new CharSize(rect.Width, rect.Height);
            }
        }
    }
}
