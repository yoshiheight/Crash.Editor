using System;
using Crash.Core.Collections;
using Crash.Core.Text;
using Crash.Editor.Engine.Model.Common.UndoRedo;
using Crash.Editor.Engine.Model.EditCommands.LowLevel;

namespace Crash.Editor.Engine.Model.EditCommands.HighLevel
{
    /// <summary>
    /// 文字列（複数行可）の挿入を行うコマンド。
    /// </summary>
    internal sealed class InsertTextCommand : IHighLevelCommand
    {
        public ModifyDetail ModifyDetail => ModifyDetail.Insert;

        public TextRange ResultRange { get; private set; }

        private readonly TextDocument.IInternalData _doc;
        private readonly TextPos _pos;
        private readonly string _value;
        private readonly UndoRedoCompositeCommand<ILowLevelCommand> _innerCommands = new UndoRedoCompositeCommand<ILowLevelCommand>();

        private readonly bool _isSingleLine;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public InsertTextCommand(TextDocument.IInternalData doc, TextPos pos, string value)
        {
            _doc = doc;
            _pos = pos;
            _value = value;
            _isSingleLine = StringUtil.IsSingleLine(_value);
        }

        public bool Validate()
        {
            return !string.IsNullOrEmpty(_value);
        }

        public void Execute()
        {
            if (_isSingleLine)
            {
                _innerCommands.Add(new InsertSingleTextCommand(_doc, _pos, _value));

                ResultRange = new TextRange(_pos, new TextPos(_pos.LineIndex, _pos.CharIndex + _value.Length));
            }
            else
            {
                var lines = TextDocument.ToLineModels(_value);
                var end = new TextPos(_pos.LineIndex + lines.LastIndex(), lines[^1].Length);

                _innerCommands.Add(new RemoveSingleTextCommand(_doc, _pos));
                _innerCommands.Add(new InsertSingleTextCommand(_doc, _pos, lines[0].Text));
                _innerCommands.Add(new InsertLinesCommand(_doc, _pos.LineIndex + 1, lines.AsMemory()[1..]));
                _innerCommands.Add(new InsertSingleTextCommand(_doc, end, _doc.LineList[_pos.LineIndex].Sub(_pos.CharIndex)));

                ResultRange = new TextRange(_pos, end);
            }

            _innerCommands.Execute();
        }

        public void Undo()
        {
            _innerCommands.Undo();
            _innerCommands.Clear();

            ResultRange = new TextRange(_pos, _pos);
        }
    }
}
