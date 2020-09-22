using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Crash.Core;
using Crash.Core.Text;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.Presenter;
using Sample.WinForms.TextEditor.UIContextImpl;

namespace Sample.WinForms.TextEditor
{
    // Visual Studioのフォームデザイナで開かれるのを防ぐため
    sealed class b6eaf0fff847432f965c47974ec1c356 { }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class TextEditorControl : UserControl
    {
        private readonly TextPresenter _presenter;

        private readonly StringBuilder _inputBuffer = new StringBuilder();

        private readonly Dictionary<Keys, Action> _keyBindings;

        internal IInputMethod InputMethod { get; private set; } = null!;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TextEditorControl()
        {
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
            MinimumSize = new Size(300, 100);

            _presenter = new TextPresenter(new CanvasContext(this), @"
{
    ""ui_colors"": {
        ""background"": ""#E8E8EC"",
        ""scrollThumb"": ""#C2C3C9"",
        ""scrollThumb_active"": ""#222222"",
        ""scrollArrow"": ""#868999"",
        ""scrollArrow_active"": ""#222222"",
        ""scrollArrow_disable"": ""#C2C3C9"",
    },
    ""fonts"": {
        ""asciiFont"": { ""name"": ""Consolas"", ""height"": 14.0 },
        ""jpFont"": { ""name"": ""メイリオ"", ""height"": 12.0 },
    },
    ""tabWidth"": 4,
    ""scrollSpeed_vertical"": 3,
    ""scrollSpeed_horizontal"": 4,
    ""lineHeightAdjust"": 0,
}");

            _keyBindings = BindKeys();

            Disposed += (s, e) => Disposer.Dispose(_presenter);
        }

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Keys, Action> BindKeys()
        {
            return new Dictionary<Keys, Action>
            {
                // カーソル移動系
                [Keys.Up] = () => _presenter.Up(),
                [Keys.Down] = () => _presenter.Down(),
                [Keys.Left] = () => _presenter.Left(),
                [Keys.Right] = () => _presenter.Right(),
                [Keys.Home] = () => _presenter.MoveLineTop(),
                [Keys.Home | Keys.Control] = () => _presenter.MoveTop(),
                [Keys.End] = () => _presenter.MoveLineEnd(),
                [Keys.End | Keys.Control] = () => _presenter.MoveEnd(),
                [Keys.PageUp] = () => _presenter.PageUp(),
                [Keys.PageDown] = () => _presenter.PageDown(),
                [Keys.Right | Keys.Control] = () => _presenter.MoveNextWord(),
                [Keys.Left | Keys.Control] = () => _presenter.MovePrevWord(),

                // カーソル移動系（選択）
                [Keys.Up | Keys.Shift] = () => _presenter.UpSelect(),
                [Keys.Down | Keys.Shift] = () => _presenter.DownSelect(),
                [Keys.Left | Keys.Shift] = () => _presenter.LeftSelect(),
                [Keys.Right | Keys.Shift] = () => _presenter.RightSelect(),
                [Keys.Home | Keys.Shift] = () => _presenter.MoveLineTopSelect(),
                [Keys.Home | Keys.Control | Keys.Shift] = () => _presenter.MoveTopSelect(),
                [Keys.End | Keys.Shift] = () => _presenter.MoveLineEndSelect(),
                [Keys.End | Keys.Control | Keys.Shift] = () => _presenter.MoveEndSelect(),
                [Keys.PageUp | Keys.Shift] = () => _presenter.PageUpSelect(),
                [Keys.PageDown | Keys.Shift] = () => _presenter.PageDownSelect(),
                [Keys.Right | Keys.Control | Keys.Shift] = () => _presenter.MoveNextWordSelect(),
                [Keys.Left | Keys.Control | Keys.Shift] = () => _presenter.MovePrevWordSelect(),

                // 編集系
                [Keys.Z | Keys.Control] = () => _presenter.Undo(),
                [Keys.Y | Keys.Control] = () => _presenter.Redo(),
                [Keys.C | Keys.Control] = () => Copy(),
                [Keys.V | Keys.Control] = () => Paste(),
                [Keys.X | Keys.Control] = () => Cut(),
                [Keys.Enter] = () => _presenter.NewLine(),
                [Keys.Delete] = () => _presenter.Delete(),
                [Keys.Back] = () => _presenter.DeleteBack(),
                [Keys.Tab] = () => _presenter.Indent(),
                [Keys.Tab | Keys.Shift] = () => _presenter.Unindent(),

                // 選択系
                [Keys.A | Keys.Control] = () => _presenter.SelectAll(),
                [Keys.Escape] = () => _presenter.ClearSelect(),

                // スクロール系
                [Keys.Up | Keys.Control] = () => _presenter.ScrollUp(),
                [Keys.Down | Keys.Control] = () => _presenter.ScrollDown(),
                [Keys.ProcessKey] = () => _presenter.ScrollCaret(), // IME入力中
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void Copy()
        {
#warning Enviromment.NewLine　にする
            if (_presenter.IsSelected)
            {
                Clipboard.SetText(_presenter.GetSelectionText());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Cut()
        {
            Copy();
            _presenter.RemoveSelectionText();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Paste()
        {
#warning クリップボード内の改行コードはなんでも受け付けるようにすること（model側で処理しているはず）
            _presenter.InsertText(Clipboard.GetText());
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool CanEnableIme => true;

        /// <summary>
        /// 
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            InputMethod = new InputMethod(Handle, () => Focused);

            base.OnHandleCreated(e);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.HasFlag(Keys.Shift))
            {
                return true;
            }

            switch (keyData)
            {
                case Keys.Tab:
                case Keys.Enter:
                case Keys.Escape:
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_keyBindings.TryGetValue(e.KeyData, out var action))
            {
                action();

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!CharUtil.CanDraw(e.KeyChar))
            {
                return;
            }

            e.Handled = true;
            var isFirstAppend = _inputBuffer.Length == 0;
            _inputBuffer.Append(e.KeyChar);

            // 日本語入力の場合に1文字毎に当該イベントが発生するが、一連の文字列として処理する為の処置
            // やや強引だが、とりあえずこの方法にしておく（正しく処理するにはIMM関連のWin32APIレベルでの対応が必要で面倒なので）
            if (isFirstAppend)
            {
                BeginInvoke((Action)(() =>
                {
                    _presenter.InsertText(_inputBuffer.ToString());
                    _inputBuffer.Clear();
                }));
            }
        }
    }
}
