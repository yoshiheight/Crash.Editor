using Crash.Editor.Engine.Model.Common.UndoRedo;
using Crash.Editor.Engine.Model.EditCommands.LowLevel;

namespace Crash.Editor.Engine.Model.EditCommands.HighLevel
{
    /// <summary>
    /// 指定範囲（複数行可）の文字列の削除を行うコマンド。
    /// </summary>
    internal sealed class RemoveTextCommand : IHighLevelCommand
    {
        public ModifyDetail ModifyDetail { get; }

        public TextRange ResultRange { get; private set; }

        private readonly TextDocument.IInternalData _doc;
        private readonly TextRange _range;
        private readonly UndoRedoCompositeCommand<ILowLevelCommand> _innerCommands = new UndoRedoCompositeCommand<ILowLevelCommand>();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RemoveTextCommand(TextDocument.IInternalData doc, TextRange range, ModifyDetail docModifyDetail)
        {
            _doc = doc;
            _range = range;
            ModifyDetail = docModifyDetail;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Validate()
        {
            return !_range.IsEmpty;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Execute()
        {
            if (_range.LineCount == 1)
            {
                _innerCommands.Add(new RemoveSingleTextCommand(_doc, _range.Start, _range.CharLength));
            }
            else
            {
                _innerCommands.Add(new RemoveSingleTextCommand(_doc, _range.Start));
                _innerCommands.Add(new InsertSingleTextCommand(_doc, _range.Start, _doc.LineList[_range.End.LineIndex].Sub(_range.End.CharIndex)));
                _innerCommands.Add(new RemoveLinesCommand(_doc, _range.Start.LineIndex + 1, _range.LineOffset));
            }

            _innerCommands.Execute();

            ResultRange = new TextRange(_range.Start, _range.Start);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Undo()
        {
            _innerCommands.Undo();
            _innerCommands.Clear();

            ResultRange = _range;
        }
    }
}
