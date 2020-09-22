using System.Buffers;
using Crash.Core;
using Crash.Core.Diagnostics;
using Crash.Core.Drawing;
using Crash.Core.UI;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.View.Common.Measurement;

namespace Crash.Editor.Engine.View.Common.Drawer
{
    /// <summary>
    /// 文字グループの描画クラス。
    /// </summary>
    internal sealed class CharGroupDrawer
    {
        private readonly ArrayBufferWriter<char> _buffer = new ArrayBufferWriter<char>(10);

        private int _indexOnLine;

        private Point2D _location;

        [Aggregation]
        private IFont _font = null!;

        private Color _color;

        public IFont Font => _font;
        public Color Color => _color;

        public void Init(ICharMetric cdi)
        {
            _buffer.Clear();

            _indexOnLine = cdi.IndexEachLine;
            _location = new Point2D((int)cdi.Left, cdi.Y);
            _buffer.GetSpan(1)[0] = cdi.Char;
            _buffer.Advance(1);
            _font = cdi.Font;
            _color = cdi.Color;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool TryAdd(ICharMetric cdi)
        {
            // 同じ種別の文字はまとめて描画する
            if (_location.Y == cdi.Y
                && cdi.IndexEachLine == _indexOnLine + 1
                && object.ReferenceEquals(_font, cdi.Font)
                && (_color == cdi.Color || _color.IsTransparent || cdi.Color.IsTransparent))
            {
                _buffer.GetSpan(1)[0] = cdi.Char;
                _buffer.Advance(1);
                _indexOnLine = cdi.IndexEachLine;
                if (_color.IsTransparent)
                {
                    _color = cdi.Color;
                }
                return true;
            }
            return false;
        }

        public void Draw(Renderer renderer)
        {
            DebugUtil.DebugCode(() =>
            {
                renderer.SetColor(Color.Red);
                renderer.FillRect(new Rect2D(_location, new Size2D(8, 2)));
                renderer.FillRect(new Rect2D(_location, new Size2D(2, 8)));
            });

            renderer.SetColor(_color);
            renderer.SetFont(_font);
            renderer.DrawString(_buffer.WrittenSpan, _location);
        }
    }
}
