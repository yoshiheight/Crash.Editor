using System.IO;
using System.Linq;
using System.Text;
using Crash.Core;
using Crash.Core.Diagnostics;
using Crash.Core.Text;
using Crash.Core.UI.UIContext;
using Crash.Editor.Engine.Model;
using Crash.Editor.Engine.View;

namespace Crash.Editor.Engine.Presenter
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TextPresenter : IAutoFieldDisposable
    {
        private readonly TextDocument _doc;

        [DisposableField]
        private readonly TextView _view;

        /// <summary>
        /// 
        /// </summary>
        public TextPresenter(ICanvasContext canvasContext, string settingsJson)
        {
            _doc = new TextDocument(settingsJson);
            _view = new TextView(canvasContext, _doc, settingsJson);

            DebugUtil.DebugCode(() =>
            {
                canvasContext.Created += _ =>
                {
                    InsertText(string.Format("あ{0}い{1}う　え\tお{2}", StringUtil.Cr, StringUtil.Lf, StringUtil.CrLf));
                    InsertText(string.Format("か{0}き{1}く　け\tこ{2}", StringUtil.Cr, StringUtil.CrLf, StringUtil.Lf));
                    InsertText(string.Format("さ{0}し{1}す　せ\tそ{2}", StringUtil.Lf, StringUtil.Cr, StringUtil.CrLf));
                    InsertText(string.Format("た{0}ち{1}つ　て\tと{2}", StringUtil.Lf, StringUtil.CrLf, StringUtil.Cr));
                    InsertText(string.Format("な{0}に{1}ぬ　ね\tの{2}", StringUtil.CrLf, StringUtil.Cr, StringUtil.Lf));
                    InsertText(string.Format("は{0}ひ{1}ふ　へ\tほ{2}", StringUtil.CrLf, StringUtil.Lf, StringUtil.Cr));
                };
            });
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [カーソル]系
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// カーソル上
        /// </summary>
        public void Up()
        {
            _view.TextArea.MoveUp(false);
        }

        /// <summary>
        /// カーソル下
        /// </summary>
        public void Down()
        {
            _view.TextArea.MoveDown(false);
        }

        /// <summary>
        /// カーソル左
        /// </summary>
        public void Left()
        {
            _view.TextArea.MoveLeft(false);
        }

        /// <summary>
        /// カーソル右
        /// </summary>
        public void Right()
        {
            _view.TextArea.MoveRight(false);
        }

        /// <summary>
        /// カーソル１ページ上
        /// </summary>
        public void PageUp()
        {
            _view.TextArea.MovePageUp(false);
        }

        /// <summary>
        /// カーソル１ページ下
        /// </summary>
        public void PageDown()
        {
            _view.TextArea.MovePageDown(false);
        }

        /// <summary>
        /// カーソル行頭
        /// </summary>
        public void MoveLineTop()
        {
            _view.TextArea.MoveLineTop(false);
        }

        /// <summary>
        /// カーソル行末
        /// </summary>
        public void MoveLineEnd()
        {
            _view.TextArea.MoveLineEnd(false);
        }

        /// <summary>
        /// カーソル文書先頭
        /// </summary>
        public void MoveTop()
        {
            _view.TextArea.MoveTop(false);
        }

        /// <summary>
        /// カーソル文書末尾
        /// </summary>
        public void MoveEnd()
        {
            _view.TextArea.MoveEnd(false);
        }

        /// <summary>
        /// カーソル任意位置
        /// </summary>
        public void MovePos(int lineIndex, int charIndex)
        {
            _view.TextArea.MovePos(new TextPos(lineIndex, charIndex), false);
        }

        /// <summary>
        /// カーソル次の単語
        /// </summary>
        public void MoveNextWord()
        {
            _view.TextArea.MoveNextWord(false);
        }

        /// <summary>
        /// カーソル前の単語
        /// </summary>
        public void MovePrevWord()
        {
            _view.TextArea.MovePrevWord(false);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [カーソル(選択)]系
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// カーソル上
        /// </summary>
        public void UpSelect()
        {
            _view.TextArea.MoveUp(true);
        }

        /// <summary>
        /// カーソル下
        /// </summary>
        public void DownSelect()
        {
            _view.TextArea.MoveDown(true);
        }

        /// <summary>
        /// カーソル左
        /// </summary>
        public void LeftSelect()
        {
            _view.TextArea.MoveLeft(true);
        }

        /// <summary>
        /// カーソル右
        /// </summary>
        public void RightSelect()
        {
            _view.TextArea.MoveRight(true);
        }

        /// <summary>
        /// カーソル１ページ上
        /// </summary>
        public void PageUpSelect()
        {
            _view.TextArea.MovePageUp(true);
        }

        /// <summary>
        /// カーソル１ページ下
        /// </summary>
        public void PageDownSelect()
        {
            _view.TextArea.MovePageDown(true);
        }

        /// <summary>
        /// カーソル行頭
        /// </summary>
        public void MoveLineTopSelect()
        {
            _view.TextArea.MoveLineTop(true);
        }

        /// <summary>
        /// カーソル行末
        /// </summary>
        public void MoveLineEndSelect()
        {
            _view.TextArea.MoveLineEnd(true);
        }

        /// <summary>
        /// カーソル文書先頭
        /// </summary>
        public void MoveTopSelect()
        {
            _view.TextArea.MoveTop(true);
        }

        /// <summary>
        /// カーソル文書末尾
        /// </summary>
        public void MoveEndSelect()
        {
            _view.TextArea.MoveEnd(true);
        }

        /// <summary>
        /// カーソル任意位置
        /// </summary>
        public void MovePosSelect(int lineIndex, int charIndex)
        {
            _view.TextArea.MovePos(new TextPos(lineIndex, charIndex), true);
        }

        /// <summary>
        /// カーソル次の単語
        /// </summary>
        public void MoveNextWordSelect()
        {
            _view.TextArea.MoveNextWord(true);
        }

        /// <summary>
        /// カーソル前の単語
        /// </summary>
        public void MovePrevWordSelect()
        {
            _view.TextArea.MovePrevWord(true);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [編集]系
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 元に戻せるかどうか
        /// </summary>
        public bool CanUndo => _doc.CanUndo;

        /// <summary>
        /// やり直せるかどうか
        /// </summary>
        public bool CanRedo => _doc.CanRedo;

        /// <summary>
        /// 元に戻す
        /// </summary>
        public void Undo()
        {
            _doc.Undo();
        }

        /// <summary>
        /// やり直し
        /// </summary>
        public void Redo()
        {
            _doc.Redo();
        }

        /// <summary>
        /// テキスト挿入
        /// </summary>
        public void InsertText(string insertText)
        {
            _doc.InsertText(_view.TextArea.SelectionRange, insertText);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public void Delete()
        {
            if (_view.TextArea.IsSelected)
            {
                _doc.RemoveText(_view.TextArea.SelectionRange);
            }
            else
            {
                _doc.RemoveChar(_view.TextArea.Caret.Pos);
            }
        }

        /// <summary>
        /// 前を削除
        /// </summary>
        public void DeleteBack()
        {
            if (_view.TextArea.IsSelected)
            {
                _doc.RemoveText(_view.TextArea.SelectionRange);
            }
            else
            {
                _doc.RemoveBack(_view.TextArea.Caret.Pos);
            }
        }

        /// <summary>
        /// 改行
        /// </summary>
        public void NewLine()
        {
            _doc.InsertText(_view.TextArea.SelectionRange, StringUtil.CrLf);
        }

        /// <summary>
        /// インデント
        /// </summary>
        public void Indent()
        {
            _doc.Indent(_view.TextArea.SelectionRange);
        }

        /// <summary>
        /// 逆インデント
        /// </summary>
        public void Unindent()
        {
            _doc.Unindent(_view.TextArea.SelectionRange);
        }

        public void RemoveSelectionText()
        {
            if (_view.TextArea.IsSelected)
            {
                _doc.RemoveText(_view.TextArea.SelectionRange);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [ファイル]系コマンド
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 初期化。
        /// </summary>
        public void NewDocument()
        {
            _doc.NewDocument();
        }

        /// <summary>
        /// 文字列設定。
        /// </summary>
        public void SetDocument(string text)
        {
            using (var reader = new TextLinesReader(new StringReader(text)))
            {
                _doc.SetDocument(reader.ReadLines());
            }
        }

        /// <summary>
        /// 変更フラグのクリア。
        /// </summary>
        public void ClearModifyCount()
        {
            _doc.ClearModifyCount();
        }

        /// <summary>
        /// 更新されているかどうか
        /// </summary>
        public bool IsModified => _doc.IsModified;

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [データ取得]系
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        ///
        /// <summary>
        /// 行数
        /// </summary>
        public int LineCount => _doc.Lines.Count;

        /// <summary>
        /// 行
        /// </summary>
        public string GetLine(int lineIndex)
        {
            return _doc.Lines[lineIndex].Text;
        }

        /// <summary>
        /// 現在行
        /// </summary>
        public string CurrentLine => _doc.Lines[_view.TextArea.Caret.Pos.LineIndex].Text;

        public int CurrentLineIndex => _view.TextArea.Caret.Pos.LineIndex;
        public int CurrentCharIndex => _view.TextArea.Caret.Pos.CharIndex;
        public int CurrentColumnIndex => _view.TextArea.Caret.CurrentColumnIndex;

        public string GetText()
        {
            var buffer = new StringBuilder();
            using (var writer = new StringWriter(buffer))
            {
                writer.WriteLines(_doc.Lines.Select(line => line.Text), StringUtil.Lf);
            }
            return buffer.ToString();
        }

        public string GetSelectionText()
        {
            var buffer = new StringBuilder();
            using (var writer = new StringWriter(buffer))
            {
                writer.WriteLines(_doc.GetStringsInRange(_view.TextArea.SelectionRange), StringUtil.Lf);
            }
            return buffer.ToString();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [選択]系
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 全選択
        /// </summary>
        public void SelectAll()
        {
            _view.TextArea.SelectAll();
        }

        /// <summary>
        /// 選択解除
        /// </summary>
        public void ClearSelect()
        {
            _view.TextArea.ClearSelect();
        }

        /// <summary>
        /// カーソル位置の単語を選択
        /// </summary>
        public void SelectWord()
        {
            _view.TextArea.SelectWord();
        }

        /// <summary>
        /// 選択されているかどうか
        /// </summary>
        public bool IsSelected => _view.TextArea.IsSelected;

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // [スクロール]系（キャレット移動を伴わない。いわゆるスクロールのみの操作）
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        public void ScrollUp()
        {
            _view.ScrollUp();
        }

        public void ScrollDown()
        {
            _view.ScrollDown();
        }

        public void ScrollCaret()
        {
            _view.ScrollCaret();
        }
    }
}
