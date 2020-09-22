using System;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI;
using Crash.Core.UI.Common;
using Crash.Editor.Engine.Model;

namespace Crash.Editor.Engine.View.Common
{
    /// <summary>
    /// キャレット。
    /// </summary>
    public sealed class Caret : IAutoFieldDisposable
    {
        /// <summary></summary>
        [Aggregation]
        private readonly TextView _textView;

        /// <summary></summary>
        [DisposableField]
        private readonly UITimer _blinkTimer;

        /// <summary></summary>
        private bool _isCaretPositive;

        /// <summary>位置。</summary>
        public Point2D Location { get; private set; }

        /// <summary>位置の絶対座標（スクロール状態を反映した値）。</summary>
        public Point2D ScrolledAbsLocation => _textView.TextArea.ToAbs(
            Location.Offset(_textView.GetScrolledSize().InvertSign()));

        /// <summary></summary>
        public TextPos Pos { get; private set; } = TextPos.Empty;

        /// <summary></summary>
        public int BaseX { get; private set; }

        public int CurrentColumnIndex { get; private set; }

        public event Action? BlinkChanged;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        internal Caret(TextView textView)
        {
            _textView = textView;

            _blinkTimer = new UITimer(500, true, () =>
            {
                _isCaretPositive = !_isCaretPositive;
                BlinkChanged?.Invoke();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Move(TextPos pos, bool isResetBaseX)
        {
            Pos = pos;

            _textView.DrawingMetricsMeasurer.CalcTotalWidth(
                Pos.LineIndex, Pos.CharIndex,
                out var totalWidth, out var totalColumnCount);

            Location = new Point2D(CharSize.RoundWidth(totalWidth), _textView.GetLineHeight() * Pos.LineIndex);
            CurrentColumnIndex = totalColumnCount;

            if (isResetBaseX)
            {
                BaseX = Location.X;
            }

            if (_textView.SharedInfo.HasFocus)
            {
                _isCaretPositive = true;
                _blinkTimer.Restart();
            }
        }

        internal void Update()
        {
            Move(Pos, true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw(Renderer renderer)
        {
            if (_textView.SharedInfo.HasFocus && _isCaretPositive)
            {
                renderer.SetColor(Color.Black);
                renderer.FillRect(new Rect2D(Location, new Size2D(2, _textView.GetLineHeight())));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateOnFocusChanged()
        {
            if (_textView.SharedInfo.HasFocus)
            {
                _isCaretPositive = true;
                _blinkTimer.Restart();
            }
            else
            {
                _isCaretPositive = false;
                _blinkTimer.Stop();
            }
        }
    }
}
