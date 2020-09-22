using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crash.Core.Collections;
using Crash.Core.Drawing;
using Crash.Core.UI.Common;
using Crash.Core.UI.UIContext;

namespace Crash.Core.UI
{
    /// <summary>
    /// UIエレメントを仮想的に実現するためのクラス。
    /// 実行環境におけるフロントエンドが提供するキャンバス内で、UIエレメントの階層的なレイアウトが可能である。
    /// 当クラスはキャンバス1つに対してカスタムコントロール1つを実装する目的で使用すること。
    /// </summary>
    public abstract class UIElement : IAutoFieldDisposable
    {
        /// <summary></summary>
        protected TreeNodeContext<UIElement> Tnc { get; }

        /// <summary>エレメント矩形（親からの相対座標）</summary>
        private Rect2D _relativeRect;

        /// <summary></summary>
        public Rect2D RelativeRect => _relativeRect;

        /// <summary></summary>
        public Rect2D ClientRect => new Rect2D(0, 0, _relativeRect.Width, _relativeRect.Height);

        public virtual Rect2D ContentRect => ClientRect;

        /// <summary></summary>
        public Size2D ClientSize => _relativeRect.Size;

        /// <summary></summary>
        protected bool IsDragging => object.ReferenceEquals(this, SharedInfo.DraggingElement);

        /// <summary></summary>
        private readonly Lazy<InternalSharedInfo> _lazySharedInfo;

        /// <summary></summary>
        public ISharedInfo SharedInfo => _lazySharedInfo.Value;

        public ICanvasContext CanvasContext => _lazySharedInfo.Value.CanvasContext;

        private readonly Region2D _clipRegion = new Region2D();

        [DisposableField]
        private ICanvasContext? _canvasContext;

        [DisposableField]
        private IOffscreen? _mainOffscreen;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected UIElement()
        {
            Tnc = TreeNodeContext<UIElement>.CreateForNormalNode(this, elem => elem.Tnc);
            _lazySharedInfo = new Lazy<InternalSharedInfo>(() => Tnc.GetRoot()._lazySharedInfo.Value, false);
        }

        /// <summary>
        /// ルート用コンストラクタ。
        /// </summary>
        protected UIElement(ICanvasContext canvasContext, string settingsJson)
        {
            var options = new JsonSerializerOptions();
            options.AllowTrailingCommas = true;
            options.Converters.Add(new JsonStringEnumConverter(null, false));
            options.Converters.Add(new JsonColorConverter());
            var settings = JsonSerializer.Deserialize<Settings>(settingsJson, options);

            Tnc = TreeNodeContext<UIElement>.CreateForRootNode(this, elem => elem.Tnc);
            _lazySharedInfo = new Lazy<InternalSharedInfo>(new InternalSharedInfo(canvasContext, this, settings));
            _canvasContext = canvasContext;

#warning 後で見直し
            _lazySharedInfo.Value.CanvasContext.Created += RaiseCreated;

        }

        public void Dispose()
        {
            foreach (var target in Tnc.Children)
            {
                Disposer.Dispose(target);
            }

            IAutoFieldDisposable.DisposeFields(this);
        }

        /// <summary>
        /// 描画を促す。
        /// </summary>
        public void RequestRender()
        {
            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target.RequestRenderLocal(target.ContentRect);
            }
        }

        /// <summary>
        /// 自分のみに描画を促す。
        /// </summary>
        public void RequestRenderLocal(Rect2D relativeRect)
        {
            if (relativeRect.IsValid)
            {
                _clipRegion.AddRect(relativeRect);

                if (!_lazySharedInfo.Value._IsInvalidating)
                {
                    _lazySharedInfo.Value._IsInvalidating = true;
                    _lazySharedInfo.Value.CanvasContext.RequestRenderFrame();
                }
            }
        }

        public Rect2D AbsoluteRect { get; private set; }

        /// <summary>
        /// エレメント領域を最上位の親における座標系で取得する。
        /// </summary>
        private Rect2D GetAbsoluteRect()
        {
            var offset = Tnc.GetParents().Aggregate(
                Size2D.Empty, (total, parent) => total.Add(parent._relativeRect.Location.ToSize()));
            return _relativeRect.Offset(offset);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool HitTest(Point2D location)
        {
            return ClientRect.Contains(location);
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract Rect2D ArrangeElement();

        /// <summary>
        /// レイアウトを再調整する。
        /// </summary>
        public void UpdateLayout()
        {
            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target._relativeRect = target.ArrangeElement();
                target.AbsoluteRect = target.GetAbsoluteRect();
            }

            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target.OnPreviewResize();
            }
            foreach (var target in Tnc.GetAllChildrenAndSelfReverse())
            {
                target.OnResize();
            }

            RequestRender();
        }

        /// <summary>
        /// 
        /// </summary>
        public Point2D ToLocal(Point2D absLocation)
        {
            return absLocation.Offset(AbsoluteRect.Location.ToSize().InvertSign());
        }

        /// <summary>
        /// 
        /// </summary>
        public Point2D ToAbs(Point2D localLocation)
        {
            return localLocation.Offset(AbsoluteRect.Location.ToSize());
        }

        /// <summary>
        /// 
        /// </summary>
        public Rect2D ToLocal(Rect2D absRect)
        {
            return absRect.Offset(AbsoluteRect.Location.ToSize().InvertSign());
        }

        /// <summary>
        /// 
        /// </summary>
        public Rect2D ToAbs(Rect2D localRect)
        {
            return localRect.Offset(AbsoluteRect.Location.ToSize());
        }

        [Noop]
        protected virtual void OnPreviewInitialize() { }

        [Noop]
        protected virtual void OnInitialize() { }

        [Noop]
        protected virtual void OnPreviewCreated() { }

        [Noop]
        protected virtual void OnCreated() { }

        [Noop]
        protected virtual void OnPreviewResize() { }

        [Noop]
        protected virtual void OnResize() { }

        [Noop]
        protected virtual void OnPreviewRender(Renderer renderer) { }

        [Noop]
        protected virtual void OnRender(Renderer renderer) { }

        [Noop]
        protected virtual void OnPreviewGotFocus() { }

        [Noop]
        protected virtual void OnGotFocus() { }

        [Noop]
        protected virtual void OnPreviewLostFocus() { }

        [Noop]
        protected virtual void OnLostFocus() { }

        [Noop]
        protected virtual void OnTargetMouseLeftButtonDown(MouseState mei) { }

        [Noop]
        protected virtual void OnTargetMouseLeftButtonUp() { }

        [Noop]
        protected virtual void OnTargetMouseLeftButtonDrag(MouseState mei) { }

        [Noop]
        protected virtual void OnPreviewMouseMove(MouseState mei, ref bool handled) { }

        [Noop]
        protected virtual void OnMouseMove(MouseState mei, ref bool handled) { }

        [Noop]
        protected virtual void OnPreviewMouseWheel(MouseState mei, ref bool handled) { }

        [Noop]
        protected virtual void OnMouseWheel(MouseState mei, ref bool handled) { }

        // 以下、トンネリングイベント（プレビューイベント）、バブリングイベントのルーティングを処理する。

        /// <summary>
        /// イベントルーティング（初期化）。
        /// </summary>
        private void RaiseCreated(Size2D size)
        {
            Verifier.Verify<InvalidOperationException>(size.Width > 0);
            Verifier.Verify<InvalidOperationException>(size.Height > 0);

            _lazySharedInfo.Value.Size = size;

            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target.OnPreviewInitialize();
            }
            foreach (var target in Tnc.GetAllChildrenAndSelfReverse())
            {
                target.OnInitialize();
            }

#warning このへん、あとで整理
            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target._relativeRect = target.ArrangeElement();
            }

            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target.OnPreviewCreated();
            }
            foreach (var target in Tnc.GetAllChildrenAndSelfReverse())
            {
                target.OnCreated();
            }

            _mainOffscreen = CanvasContext.CreateOffscreen(ClientSize);

#warning このへん、あとで整理
            RaiseResize(size);

            _lazySharedInfo.Value.CanvasContext.Resize += size => RaiseResize(size);
            _lazySharedInfo.Value.CanvasContext.RenderFrame += rc => RaiseRender(rc);

            _lazySharedInfo.Value.CanvasContext.GotFocus += RaiseGotFocus;
            _lazySharedInfo.Value.CanvasContext.LostFocus += RaiseLostFocus;

            _lazySharedInfo.Value.CanvasContext.MouseDown += RaiseMouseDown;
            _lazySharedInfo.Value.CanvasContext.MouseUp += RaiseMouseUp;
            _lazySharedInfo.Value.CanvasContext.MouseMove += RaiseMouseMove;
            _lazySharedInfo.Value.CanvasContext.MouseWheel += RaiseMouseWheel;
        }

        /// <summary>
        /// イベントルーティング（リサイズ）。
        /// </summary>
        private void RaiseResize(Size2D size)
        {
            Verifier.Verify<ArgumentException>(size.Width > 0 && size.Height > 0);

            _lazySharedInfo.Value.Size = size;
            _mainOffscreen!.Resize(size);

            UpdateLayout();
        }

        /// <summary>
        /// イベントルーティング（レンダリング）。
        /// </summary>
        private void RaiseRender(ICanvasContext.IRenderingContext rc)
        {
            _lazySharedInfo.Value._IsInvalidating = false;

            foreach (var target in Tnc.GetAllChildrenAndSelf()
                .Where(target => target._clipRegion.AsReadOnly().Bounds.IsValid))
            {
                var renderer = new Renderer(_mainOffscreen!, target.AbsoluteRect, target._clipRegion);
                target.OnPreviewRender(renderer);
            }

            foreach (var target in Tnc.GetAllChildrenAndSelfReverse()
                .Where(target => target._clipRegion.AsReadOnly().Bounds.IsValid))
            {
                var renderer = new Renderer(_mainOffscreen!, target.AbsoluteRect, target._clipRegion);
                target.OnRender(renderer);
            }

            rc.TransferScreen(_mainOffscreen!);

            // 描画イベント中に「再描画要求」を出すのはNG
            Verifier.Verify<InvalidOperationException>(!_lazySharedInfo.Value._IsInvalidating);

            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target._clipRegion.Clear();
            }
        }

        /// <summary>
        /// イベントルーティング（フォーカス取得）。
        /// </summary>
        private void RaiseGotFocus()
        {
            _lazySharedInfo.Value.HasFocus = true;

            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target.OnPreviewGotFocus();
            }
            foreach (var target in Tnc.GetAllChildrenAndSelfReverse())
            {
                target.OnGotFocus();
            }
        }

        /// <summary>
        /// イベントルーティング（フォーカス消失）。
        /// </summary>
        private void RaiseLostFocus()
        {
            _lazySharedInfo.Value.HasFocus = false;

            RaiseMouseLeftButtonUp();

            foreach (var target in Tnc.GetAllChildrenAndSelf())
            {
                target.OnPreviewLostFocus();
            }
            foreach (var target in Tnc.GetAllChildrenAndSelfReverse())
            {
                target.OnLostFocus();
            }
        }

        /// <summary>
        /// イベントルーティング（マウスダウン）。
        /// </summary>
        private void RaiseMouseDown(MouseState e)
        {
            if (GetHitTestedElementsBubble(e.Location).FirstOrNull() is { } target)
            {
                if (e.IsLeft)
                {
                    _lazySharedInfo.Value.DraggingElement = target;
                    _lazySharedInfo.Value._dragStartAbsLocation = e.Location;
                    _lazySharedInfo.Value._dragCurrentAbsLocation = e.Location;

                    target.OnTargetMouseLeftButtonDown(e);
                }
            }
        }

        /// <summary>
        /// イベントルーティング（マウスアップ）。
        /// </summary>
        private void RaiseMouseUp(MouseState e)
        {
            if (e.IsLeft)
            {
                RaiseMouseLeftButtonUp();
            }
        }

        /// <summary>
        /// イベントルーティング（マウス左ボタンアップ）。
        /// </summary>
        private void RaiseMouseLeftButtonUp()
        {
            var element = _lazySharedInfo.Value.DraggingElement;
            _lazySharedInfo.Value.DraggingElement = null;
            element?.OnTargetMouseLeftButtonUp();
        }

        /// <summary>
        /// イベントルーティング（マウス移動）。
        /// </summary>
        private void RaiseMouseMove(MouseState e)
        {
            if (_lazySharedInfo.Value.DraggingElement != null)
            {
                _lazySharedInfo.Value._dragCurrentAbsLocation = e.Location;
                _lazySharedInfo.Value.DraggingElement.OnTargetMouseLeftButtonDrag(e);
            }
            else
            {
                var handled = false;
                foreach (var target in GetHitTestedElementsTunnel(e.Location))
                {
                    target.OnPreviewMouseMove(e, ref handled);
                    if (handled)
                    {
                        return;
                    }
                }
                foreach (var target in GetHitTestedElementsBubble(e.Location))
                {
                    target.OnMouseMove(e, ref handled);
                    if (handled)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// イベントルーティング（マウスホイール回転）。
        /// </summary>
        private void RaiseMouseWheel(MouseState e)
        {
            var handled = false;
            foreach (var target in GetHitTestedElementsTunnel(e.Location))
            {
                target.OnPreviewMouseWheel(e, ref handled);
                if (handled)
                {
                    return;
                }
            }
            foreach (var target in GetHitTestedElementsBubble(e.Location))
            {
                target.OnMouseWheel(e, ref handled);
                if (handled)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 指定座標のヒットテスト。
        /// </summary>
        private IEnumerable<UIElement> GetHitTestedElementsTunnel(Point2D absLocation)
        {
            return Tnc.GetAllChildrenAndSelf().Where(elem => elem.HitTest(elem.ToLocal(absLocation)));
        }

        /// <summary>
        /// 指定座標のヒットテスト。
        /// </summary>
        private IEnumerable<UIElement> GetHitTestedElementsBubble(Point2D absLocation)
        {
            return Tnc.GetAllChildrenAndSelfReverse().Where(elem => elem.HitTest(elem.ToLocal(absLocation)));
        }

        /// <summary>
        /// 
        /// </summary>
        public interface ISharedInfo
        {
            UIElement Root { get; }

            /// <summary></summary>
            Size2D DragMoveOffset { get; }

            /// <summary></summary>
            UIElement? DraggingElement { get; }

            /// <summary></summary>
            bool HasFocus { get; }

            Size2D Size { get; }

            /// <summary></summary>
            Settings Settings { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class InternalSharedInfo : ISharedInfo
        {
            /// <summary></summary>
            [field: Aggregation]
            public UIElement Root { get; }

            /// <summary></summary>
            public Point2D _dragStartAbsLocation = Point2D.Origin;

            /// <summary></summary>
            public Point2D _dragCurrentAbsLocation = Point2D.Origin;

            /// <summary></summary>
            public Size2D DragMoveOffset => _dragCurrentAbsLocation.Subtract(_dragStartAbsLocation);

            /// <summary></summary>
            public UIElement? DraggingElement { get; set; }

            /// <summary></summary>
            public bool HasFocus { get; set; }

            public Size2D Size { get; set; }

            /// <summary></summary>
            public Settings Settings { get; }

            /// <summary></summary>
            [field: Aggregation]
            public ICanvasContext CanvasContext { get; }

            public bool _IsInvalidating;

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public InternalSharedInfo(ICanvasContext canvasContext, UIElement root, Settings settings)
            {
                Settings = settings;
                CanvasContext = canvasContext;
                Root = root;
            }
        }
    }
}
