using Crash.Core.Drawing;
using Crash.Core.UI;
using Crash.Editor.Engine.View.Common.Measurement;

namespace Crash.Editor.Engine.View.Common.Drawer
{
    /// <summary>
    /// 各行の選択範囲の描画クラス。
    /// </summary>
    internal sealed class SelectionGroupDrawer
    {
        private Rect2D _rect;
        private int _indexOnLine;

        public void Init(ICharMetric cdi)
        {
            _rect = cdi.CharRect;
            _indexOnLine = cdi.IndexEachLine;
        }

        public bool TryAdd(ICharMetric cdi)
        {
            if (_rect.Y == cdi.CharRect.Y
                && cdi.IndexEachLine == _indexOnLine + 1)
            {
                _rect = _rect.Union(cdi.CharRect);
                _indexOnLine = cdi.IndexEachLine;
                return true;
            }
            return false;
        }

        public void Draw(Renderer renderer, bool hasFocus)
        {
            renderer.SetColor(hasFocus ? Color.FromRgba(0x5da6dfA0) : Color.FromRgba(0xa8cfe8A0));
            renderer.FillRect(_rect);
        }
    }
}
