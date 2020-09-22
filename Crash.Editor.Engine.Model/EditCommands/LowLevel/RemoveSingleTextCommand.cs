namespace Crash.Editor.Engine.Model.EditCommands.LowLevel
{
    /// <summary>
    /// 単一行で文字列削除を行うコマンド
    /// </summary>
    internal sealed class RemoveSingleTextCommand : ILowLevelCommand
    {
        private readonly TextDocument.IInternalData _doc;
        private readonly TextPos _pos;
        private readonly int _count;
        private string? _value;

        public RemoveSingleTextCommand(TextDocument.IInternalData doc, TextPos pos, int count = TextLine.ToEnd)
        {
            _doc = doc;
            _pos = pos;
            _count = count;
        }

        public void Execute()
        {
            var line = _doc.LineList[_pos.LineIndex];
            _value = line.Cut(_pos.CharIndex, _count);

            line.ModifyCount++;
            _doc.ModifyCount++;
        }

        public void Undo()
        {
            var line = _doc.LineList[_pos.LineIndex];
            line.Insert(_pos.CharIndex, _value!);
            _value = null;

            line.ModifyCount--;
            _doc.ModifyCount--;
        }
    }
}
