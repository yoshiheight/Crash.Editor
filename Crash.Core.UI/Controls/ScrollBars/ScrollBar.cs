using System;
using System.Collections.Generic;
using System.Linq;
using Crash.Core.Drawing;
using Crash.Core.UI.Controls.ScrollBars.Markers;

namespace Crash.Core.UI.Controls.ScrollBars
{
    /// <summary>
    /// スクロールバー。
    /// </summary>
    public sealed class ScrollBar : UIElement
    {
        /// <summary></summary>
        public event ScrollEventHandler? ValueChanged;

        /// <summary></summary>
        public static readonly int Thickness = 17;

        /// <summary></summary>
        private int _value;

        /// <summary></summary>
        public int Minimum { get; private set; }

        /// <summary></summary>
        public int Maximum { get; private set; }

        /// <summary>スクロール対象領域の全体の長さ。</summary>
        public int ContentLength => Maximum - Minimum + 1;

        /// <summary>スクロールボタンを押下した際のスクロール値。</summary>
        public int ArrowStep { get; private set; }

        /// <summary>スクロールエリアを押下した際のスクロール値（つまり現在表示中の領域の長さ）。</summary>
        public int ViewportLength { get; private set; }

        /// <summary>ユーザー操作によるスクロールの最大値。</summary>
        public int MaximumByScroll => ContentLength - ViewportLength;

        /// <summary>スクロールの現在値。</summary>
        public int Value
        {
            get => _value;
            set => SetValue(value, false);
        }

        /// <summary>スクロールバーの操作が可能かどうか。</summary>
        public bool IsScrollEnabled => ContentLength >= 2 && ArrowStep > 0 && ViewportLength > 0 && ContentLength > ViewportLength;

        /// <summary>つまみによる操作が可能かどうか。</summary>
        public bool IsThumbEnabled => IsScrollEnabled && Body.ClientSize.Height >= 15;

        /// <summary>マーカー表示が可能かどうか。</summary>
        public bool IsMarkerEnabled => Body.ClientSize.Height >= 15;

        public IEnumerable<IMarker> Marks { get; private set; } = Enumerable.Empty<IMarker>();

        /// <summary></summary>
        public ScrollBody Body { get; }

        /// <summary></summary>
        public ScrollButton PrevButton { get; }

        /// <summary></summary>
        public ScrollButton NextButton { get; }

        /// <summary></summary>
        public ScrollDirection Direction { get; }

        /// <summary>
        /// 
        /// </summary>
        public ScrollBar(ScrollDirection direction)
        {
            Direction = direction;

            Tnc.AddNode(Body = new ScrollBody(Direction));
            Tnc.AddNode(PrevButton = new ScrollButton(Direction, true));
            Tnc.AddNode(NextButton = new ScrollButton(Direction, false));
        }

        /// <summary>
        /// 
        /// </summary>
        public void SettingScrollByContentLength(int contentLength, int arrowStep, int viewportLength)
        {
            SettingScroll(0, contentLength - 1, arrowStep, viewportLength);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SettingScroll(int minimum, int maximum, int arrowStep, int viewportLength)
        {
            Verifier.Verify<ArgumentException>(maximum > minimum);
            Verifier.Verify<ArgumentException>(arrowStep > 0);
            Verifier.Verify<ArgumentException>(viewportLength > 0);
            Verifier.Verify<ArgumentException>(viewportLength >= arrowStep);

            if ((Minimum, Maximum, ArrowStep, ViewportLength) != (minimum, maximum, arrowStep, viewportLength))
            {
                (Minimum, Maximum, ArrowStep, ViewportLength) = (minimum, maximum, arrowStep, viewportLength);
                SetValue(_value, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetValue(int newValue, bool isForceUpdateLayout)
        {
            var oldValue = _value;
            _value = ToNormalizedValue(newValue);
            if (_value != oldValue)
            {
                ValueChanged?.Invoke(_value);
            }

            if (_value != oldValue || isForceUpdateLayout)
            {
                UpdateLayout();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int ToNormalizedValue(int value)
        {
            return IsScrollEnabled ? MathUtil.Clamp(value, Minimum, MaximumByScroll) : Minimum;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsInRangeValue(int value)
        {
            return MathUtil.IsInRange(value, Minimum, Maximum);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsInRangeValueByScroll(int value)
        {
            return MathUtil.IsInRange(value, Minimum, MaximumByScroll);
        }

        public void SetMarks(IEnumerable<IMarker> marks)
        {
            Marks = marks;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Rect2D ArrangeElement()
        {
            switch (Direction)
            {
                case ScrollDirection.Vertical:
                    var left = Tnc.GetParent().ClientSize.Width - ScrollBar.Thickness;
                    var height = Tnc.GetParent().ClientSize.Height - ScrollBar.Thickness;
                    return new Rect2D(left, 0, ScrollBar.Thickness, height);
                case ScrollDirection.Horizontal:
                    var top = Tnc.GetParent().ClientSize.Height - ScrollBar.Thickness;
                    var width = Tnc.GetParent().ClientSize.Width - ScrollBar.Thickness;
                    return new Rect2D(0, top, width, ScrollBar.Thickness);
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public delegate void ScrollEventHandler(int value);

    /// <summary>
    /// 
    /// </summary>
    public enum ScrollDirection
    {
        Vertical,
        Horizontal,
    }
}
