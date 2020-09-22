using System;

namespace Crash.Editor.Engine.Model.EditCommands.LowLevel
{
    /// <summary>
    /// 複数行の挿入を行うコマンド
    /// </summary>
    internal sealed class InsertLinesCommand : ILowLevelCommand
    {
        private readonly TextDocument.IInternalData _doc;
        private readonly int _lineIndex;
        private readonly int _count;
        private ReadOnlyMemory<TextLine> _lines;

        public InsertLinesCommand(TextDocument.IInternalData doc, int lineIndex, Memory<TextLine> lines)
        {
            _doc = doc;
            _lineIndex = lineIndex;
            _lines = lines;
            _count = _lines.Length;
        }

        public void Execute()
        {
            foreach (var line in _lines.Span)
            {
                line.ModifyCount++;
            }
            _doc.LineList.InsertRange(_lineIndex, _lines.Span);
            _lines = null;
            _doc.ModifyCount++;
        }

        public void Undo()
        {
            _lines = _doc.LineList.SliceToArray(_lineIndex, _count);
            foreach (var line in _lines.Span)
            {
                line.ModifyCount--;
            }
            _doc.LineList.RemoveRange(_lineIndex, _lines.Length);
            _doc.ModifyCount--;
        }
    }
}
