﻿using System.Linq;
using Crash.Core.Collections;
using Crash.Core.Text;
using Crash.Editor.Engine.Model.Common.UndoRedo;
using Crash.Editor.Engine.Model.EditCommands.LowLevel;

namespace Crash.Editor.Engine.Model.EditCommands.HighLevel
{
    /// <summary>
    /// 指定範囲（複数行可）の行全体のインデント解除を行うコマンド。
    /// </summary>
    internal sealed class UnindentCommand : IHighLevelCommand
    {
        public ModifyDetail ModifyDetail => ModifyDetail.IndentOrUnindent;

        public TextRange ResultRange { get; private set; }

        private readonly TextDocument.IInternalData _doc;
        private readonly TextRange _range;
        private readonly UndoRedoCompositeCommand<ILowLevelCommand> _innerCommands = new UndoRedoCompositeCommand<ILowLevelCommand>();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public UnindentCommand(TextDocument.IInternalData doc, TextRange range)
        {
            _doc = doc;
            _range = range;
        }

        public bool Validate()
        {
            return _range.Start.LineIndex != _range.End.LineIndex;
        }

        public void Execute()
        {
            var start = _range.OriginalStart;
            var end = _range.OriginalEnd;
            foreach (var i in _range.GetLineIndexes()
                .Where(i => _doc.LineList[i].Text.FirstOrNullValue() == CharUtil.Tab))
            {
                _innerCommands.Add(new RemoveSingleTextCommand(_doc, new TextPos(i, 0), 1));
                if (i == start.LineIndex)
                {
                    start = new TextPos(start.LineIndex, start.CharIndex - 1);
                }
                if (i == end.LineIndex)
                {
                    end = new TextPos(end.LineIndex, end.CharIndex - 1);
                }
            }

            _innerCommands.Execute();

            ResultRange = new TextRange(start, end);
        }

        public void Undo()
        {
            _innerCommands.Undo();
            _innerCommands.Clear();

            ResultRange = _range;
        }
    }
}
