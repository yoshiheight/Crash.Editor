using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crash.Core;
using Crash.Core.Drawing;
using Crash.Core.UI;
using Crash.Core.UI.Common;
using Crash.Core.UI.Controls.ScrollBars;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.Model;
using Crash.Editor.Engine.View.Common.Measurement;

namespace Crash.Editor.Engine.View
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TextView : UIElement
    {
        /// <summary></summary>
        [field: Aggregation]
        public IReadOnlyTextDocument Doc { get; }

        /// <summary></summary>
        public TextArea TextArea { get; } = new TextArea();

        /// <summary></summary>
        public VRuler VRuler { get; } = new VRuler();

        /// <summary></summary>
        public ScrollBar VScrollBar { get; } = new ScrollBar(ScrollDirection.Vertical);

        /// <summary></summary>
        public ScrollBar HScrollBar { get; } = new ScrollBar(ScrollDirection.Horizontal);

        [field: DisposableField]
        public IFont AsciiFont { get; private set; } = null!;

        [field: DisposableField]
        public IFont JpFont { get; private set; } = null!;

        public DrawingMetricsMeasure DrawingMetricsMeasurer { get; }

        internal readonly Settings _settings;

        internal double ZoomRate { get; private set; } = 1.0;

        private int _zoomLevel;

        internal Action? FontChanged;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TextView(ICanvasContext canvasContext, IReadOnlyTextDocument doc, string settingsJson)
            : base(canvasContext, settingsJson)
        {
            var options = new JsonSerializerOptions();
            options.AllowTrailingCommas = true;
            options.Converters.Add(new JsonStringEnumConverter(null, false));
            _settings = JsonSerializer.Deserialize<Settings>(settingsJson, options);

            DrawingMetricsMeasurer = new DrawingMetricsMeasure(this);

            Doc = doc;

            Tnc.AddNode(TextArea);
            Tnc.AddNode(VRuler);
            Tnc.AddNode(VScrollBar);
            Tnc.AddNode(HScrollBar);

            VScrollBar.ValueChanged += value =>
            {
                VRuler.RequestRenderByVScroll();
                TextArea.RequestRenderByVScroll();
            };
            HScrollBar.ValueChanged += value =>
            {
                TextArea.RequestRenderByHScroll();
                TextArea.RequestRenderCurrentLine();
            };
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewMouseWheel(MouseState mouseState, ref bool handled)
        {
            if (mouseState.IsCtrl)
            {
                _zoomLevel += mouseState.WheelNotchY;
#warning とりあえず
                _zoomLevel = MathUtil.Clamp(_zoomLevel, -10, 22);
                ZoomRate = Math.Pow(1.1, _zoomLevel);

                AsciiFont.Dispose();
                AsciiFont = CanvasContext.CreateFont(_settings.fonts.asciiFont.name, _settings.fonts.asciiFont.height * ZoomRate);
                JpFont.Dispose();
                JpFont = CanvasContext.CreateFont(_settings.fonts.jpFont.name, _settings.fonts.jpFont.height * ZoomRate);

                UpdateLayout();

                FontChanged?.Invoke();
            }
            else
            {
                if (mouseState.IsShift)
                {
                    HScrollBar.Value += mouseState.WheelNotchY * _settings.scrollSpeed_horizontal * -1;
                }
                else
                {
                    VScrollBar.Value += mouseState.WheelNotchY * _settings.scrollSpeed_vertical * -1;
                }
            }

            handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Rect2D ArrangeElement()
        {
            //SettingScroll();

            return new Rect2D(0, 0, SharedInfo.Size.Width, SharedInfo.Size.Height);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPreviewInitialize()
        {
            Doc.DocModified += OnDocModified;

            AsciiFont = CanvasContext.CreateFont(_settings.fonts.asciiFont.name, _settings.fonts.asciiFont.height);
            JpFont = CanvasContext.CreateFont(_settings.fonts.jpFont.name, _settings.fonts.jpFont.height);
        }

        protected override void OnPreviewRender(Renderer renderer)
        {
            renderer.SetColor(Color.White);
            renderer.FillRect(ClientRect);
        }

        private void OnDocModified(ModifyEventArgs e)
        {
            SettingScroll();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnResize()
        {
            SettingScroll();
        }

        protected override void OnMouseMove(MouseState mei, ref bool handled)
        {
            CanvasContext.SetCursor(Cursor.Default);
            handled = true;
        }

        internal int HScrollStep => CharSize.RoundWidth(AsciiFont.MeasureChar('A').Width);

        internal int VScrollStep => GetLineHeight();

        internal int GetLineHeight()
        {
            return CharSize.RoundHeight(
                Math.Max(AsciiFont.MeasureChar('A').Height, JpFont.MeasureChar('あ').Height) + _settings.lineHeightAdjust);
        }

        internal int GetHScrolledWidth()
        {
            return HScrollStep * HScrollBar.Value;
        }

        internal int GetVScrolledHeight()
        {
            return GetLineHeight() * VScrollBar.Value;
        }

        internal Size2D GetScrolledSize()
        {
            return new Size2D(GetHScrolledWidth(), GetVScrolledHeight());
        }

        /// <summary>
        /// スクロールバー設定。
        /// </summary>
        private void SettingScroll()
        {
            settingVScroll();
            settingHScroll();

            // 垂直スクロール設定
            void settingVScroll()
            {
                var viewportLength = TextArea.ClientSize.Height / GetLineHeight();
                viewportLength = MathUtil.ClampMin(viewportLength, 1);
                VScrollBar.SettingScrollByContentLength(TextArea.VContentLength, 1, viewportLength);
            }

            // 水平スクロール設定
            void settingHScroll()
            {
                var viewportLength = TextArea.ClientSize.Width / HScrollStep;
                viewportLength = MathUtil.ClampMin(viewportLength, 1);
                HScrollBar.SettingScrollByContentLength(TextArea.HContentLength, 1, viewportLength);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScrollCaret()
        {
            // 縦スクロール
            var caretLineIndex = TextArea.Caret.Pos.LineIndex;
            var displayEndLineIndex = VScrollBar.Value + VScrollBar.ViewportLength - 1;
            if (displayEndLineIndex < caretLineIndex)
            {
                VScrollBar.Value += caretLineIndex - displayEndLineIndex;
            }
            else if (caretLineIndex < VScrollBar.Value)
            {
                VScrollBar.Value += caretLineIndex - VScrollBar.Value;
            }

            // 横スクロール
            var currentColumn = TextArea.Caret.Location.X / HScrollStep;
            var displayEndColumn = HScrollBar.Value + HScrollBar.ViewportLength;
            if (displayEndColumn - 3 < currentColumn)
            {
                HScrollBar.Value += currentColumn - (displayEndColumn - 7);
            }
            else if (currentColumn < HScrollBar.Value + 3)
            {
                HScrollBar.Value += currentColumn - (HScrollBar.Value + 7);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScrollUp()
        {
            VScrollBar.Value--;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScrollDown()
        {
            VScrollBar.Value++;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScrollPageUp()
        {
            VScrollBar.Value -= VScrollBar.ViewportLength;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScrollPageDown()
        {
            VScrollBar.Value += VScrollBar.ViewportLength;
        }
    }
}
