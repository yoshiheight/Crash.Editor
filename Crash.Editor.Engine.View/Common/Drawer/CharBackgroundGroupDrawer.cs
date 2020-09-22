using Crash.Core.Drawing;
using Crash.Core.UI;
using Crash.Editor.Engine.View.Common.Measurement;

namespace Crash.Editor.Engine.View.Common.Drawer
{
    /// <summary>
    /// 文字の背景色グループの描画クラス。
    /// </summary>
    internal sealed class CharBackgroundGroupDrawer
    {
        private Color _color;
        private Rect2D _rect;
        private int _indexOnLine;

        public void Init(ICharMetric cdi)
        {
            _color = cdi.BackgroundColor;
            _rect = cdi.CharRect;
            _indexOnLine = cdi.IndexEachLine;
        }

        public bool TryAdd(ICharMetric cdi)
        {
            if (_rect.Y == cdi.CharRect.Y
                && cdi.IndexEachLine == _indexOnLine + 1
                && cdi.BackgroundColor == _color)
            {
                _rect = _rect.Union(cdi.CharRect);
                _indexOnLine = cdi.IndexEachLine;
                return true;
            }
            return false;
        }

        public void Draw(Renderer renderer)
        {
            renderer.SetColor(_color);
            renderer.FillRect(_rect);
        }
    }
}
