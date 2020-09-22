using System;

namespace Crash.Editor.Engine.Model.EditCommands.LowLevel
{
    /// <summary>
    /// 複数行の削除を行うコマンド
    /// </summary>
    internal sealed class RemoveLinesCommand : ILowLevelCommand
    {
        private readonly TextDocument.IInternalData _doc;
        private readonly int _lineIndex;
        private readonly int _count;
        private TextLine[]? _lines;

        public RemoveLinesCommand(TextDocument.IInternalData doc, int lineIndex, int count)
        {
            _doc = doc;
            _lineIndex = lineIndex;
            _count = count;
        }

        public void Execute()
        {
            _lines = _doc.LineList.SliceToArray(_lineIndex, _count);
            _doc.LineList.RemoveRange(_lineIndex, _count);

            _doc.ModifyCount++;
        }

        public void Undo()
        {
            _doc.LineList.InsertRange(_lineIndex, _lines!.AsSpan());
            _lines = null;

            _doc.ModifyCount--;
        }
    }
}
