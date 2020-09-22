using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crash.Core;
using Crash.Core.Collections;
using Crash.Core.Text;
using Crash.Editor.Engine.Model.Common.UndoRedo;
using Crash.Editor.Engine.Model.EditCommands.HighLevel;

namespace Crash.Editor.Engine.Model
{
    /// <summary>
    /// テキストドキュメントを扱うクラス。
    /// </summary>
    public sealed class TextDocument : IReadOnlyTextDocument
    {
        /// <summary></summary>
        public event DocModifyEventHandler? DocModified;

        /// <summary></summary>
        private readonly UndoRedoHistory<IHighLevelCommand> _undoRedoHistory = new UndoRedoHistory<IHighLevelCommand>();

        /// <summary></summary>
        private readonly GapBuffer<TextLine> _lineList = new GapBuffer<TextLine>();

        /// <summary></summary>
        private int _modifyCount;

        /// <summary></summary>
        public bool IsModified => _modifyCount != 0;

        /// <summary></summary>
        public bool CanUndo => _undoRedoHistory.CanUndo;

        /// <summary></summary>
        public bool CanRedo => _undoRedoHistory.CanRedo;

        private Settings _settings;

        private readonly InternalData _internalData;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TextDocument(string settingsJson)
        {
            var options = new JsonSerializerOptions();
            options.AllowTrailingCommas = true;
            options.Converters.Add(new JsonStringEnumConverter(null, false));
            _settings = JsonSerializer.Deserialize<Settings>(settingsJson, options);

            _internalData = new InternalData(this);

            NewDocument();
        }

        /// <summary>1行以上の行リスト。</summary>
        public IReadOnlyList<IReadOnlyTextLine> Lines => _lineList;

        public IEnumerable<string> GetStringsInRange(TextRange range)
        {
            return range.GetLineIndexes().Select(i =>
                (range.LineCount == 1) ? _lineList[i].Sub(range.Start.CharIndex, range.CharLength)
                : (i == range.Start.LineIndex) ? _lineList[i].Sub(range.Start.CharIndex)
                : (i == range.End.LineIndex) ? _lineList[i].Sub(0, range.End.CharIndex)
                : _lineList[i].Text);
        }

        /// <summary>
        /// 初期化する。
        /// </summary>
        public void NewDocument()
        {
            SetDocument(new[] { string.Empty });

            // TODO Modifyイベント発行
        }

        /// <summary>
        /// 1行以上のデータで初期化する。
        /// </summary>
        public void SetDocument(IEnumerable<string> lines)
        {
            _modifyCount = 0;
            _undoRedoHistory.Clear();
            _undoRedoHistory.TrimExcess();
            _lineList.Clear();
            _lineList.AddRange(lines.Select(line => new TextLine(line)));
            _lineList.TrimExcess();

            Verifier.Verify<ArgumentException>(_lineList.Any());

            // TODO Modifyイベント発行
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearModifyCount()
        {
            _modifyCount = 0;
            foreach (var line in _lineList)
            {
                line.ClearModifyCount();
            }

            // TODO Modifyイベント発行
        }

        /// <summary>
        /// 
        /// </summary>
        public void InsertText(TextRange range, string text)
        {
            var composite = new HighLevelCompositeCommand();
            composite.TryAdd(new RemoveTextCommand(_internalData, range, ModifyDetail.Remove));
            composite.TryAdd(new InsertTextCommand(_internalData, range.Start, text));
            TryExecuteCommand(composite);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InsertText(TextPos pos, string text)
        {
            InsertText(new TextRange(pos, pos), text);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveText(TextRange range)
        {
            TryExecuteCommand(new RemoveTextCommand(_internalData, range, ModifyDetail.Remove));
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveChar(TextPos pos)
        {
            TryExecuteCommand(new RemoveTextCommand(_internalData, GetNextOrPrevRange(pos, true), ModifyDetail.RemoveOne));
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveBack(TextPos pos)
        {
            TryExecuteCommand(new RemoveTextCommand(_internalData, GetNextOrPrevRange(pos, false), ModifyDetail.RemoveOne));
        }

        /// <summary>
        /// 
        /// </summary>
        private TextRange GetNextOrPrevRange(TextPos pos, bool isRemoveNext)
        {
            var offsetPos = isRemoveNext ? pos.GetNextPos(this) : pos.GetPrevPos(this);
            return new TextRange(offsetPos, pos);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Undo()
        {
            var endPos = TextPos.GetEndPos(this);
            if (_undoRedoHistory.TryUndo(out var command))
            {
                RaiseModifyEvent(ModifyStatus.Undo, command, endPos);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Redo()
        {
            var endPos = TextPos.GetEndPos(this);
            if (_undoRedoHistory.TryRedo(out var command))
            {
                RaiseModifyEvent(ModifyStatus.Modify, command, endPos);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Indent(TextRange range)
        {
            if (!TryExecuteCommand(new IndentCommand(_internalData, range)))
            {
                InsertText(range, StringUtil.Tab);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unindent(TextRange range)
        {
            TryExecuteCommand(new UnindentCommand(_internalData, range));
        }

        /// <summary>
        /// 
        /// </summary>
        private bool TryExecuteCommand(IHighLevelCommand command)
        {
            if (command.Validate())
            {
                var endPos = TextPos.GetEndPos(this);
                _undoRedoHistory.Execute(command);
                RaiseModifyEvent(ModifyStatus.Modify, command, endPos);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void RaiseModifyEvent(ModifyStatus status, IHighLevelCommand command, TextPos endPos)
        {
            DocModified?.Invoke(new ModifyEventArgs(status, command, endPos));
        }

        /// <summary>
        /// 
        /// </summary>
        internal static TextLine[] ToLineModels(string text)
        {
            using (var reader = new TextLinesReader(new StringReader(text)))
            {
                return reader.ReadLines()
                    .Select(line => new TextLine(line))
                    .ToArray();
            }
        }

        internal interface IInternalData
        {
            GapBuffer<TextLine> LineList { get; }

            ref int ModifyCount { get; }
        }

        /// <summary>
        /// 内部データの直接操作を限定的に提供する。
        /// </summary>
        private sealed class InternalData : IInternalData
        {
            [Aggregation]
            private readonly TextDocument _doc;

            public GapBuffer<TextLine> LineList => _doc._lineList;

            public ref int ModifyCount => ref _doc._modifyCount;

            /// <summary>
            /// 
            /// </summary>
            public InternalData(TextDocument doc)
            {
                _doc = doc;
            }
        }
    }
}
