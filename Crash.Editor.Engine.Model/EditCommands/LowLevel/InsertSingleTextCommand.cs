namespace Crash.Editor.Engine.Model.EditCommands.LowLevel
{
    /// <summary>
    /// 単一行へ文字列挿入を行うコマンド
    /// </summary>
    internal sealed class InsertSingleTextCommand : ILowLevelCommand
    {
        private readonly TextDocument.IInternalData _doc;
        private readonly TextPos _pos;
        private string? _value;
        private readonly int _length;

        public InsertSingleTextCommand(TextDocument.IInternalData doc, TextPos pos, string value)
        {
            _doc = doc;
            _pos = pos;
            _value = value;
            _length = value.Length;
        }

        public void Execute()
        {
            var line = _doc.LineList[_pos.LineIndex];
            line.Insert(_pos.CharIndex, _value!);
            _value = null;

            line.ModifyCount++;
            _doc.ModifyCount++;
        }

        public void Undo()
        {
            var line = _doc.LineList[_pos.LineIndex];
            _value = line.Sub(_pos.CharIndex, _length);
            line.Remove(_pos.CharIndex, _length);

            line.ModifyCount--;
            _doc.ModifyCount--;
        }
    }
}
