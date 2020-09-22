using System.Collections.Generic;
using Crash.Core;
using Crash.Editor.Engine.Model.EditCommands.HighLevel;

namespace Crash.Editor.Engine.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IReadOnlyTextDocument
    {
        event DocModifyEventHandler DocModified;

        /// <summary>1行以上の行リスト。</summary>
        IReadOnlyList<IReadOnlyTextLine> Lines { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ModifyStatus
    {
        Modify,
        Undo,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ModifyDetail
    {
        Insert,
        Remove,
        RemoveOne,
        IndentOrUnindent,
    }

    public delegate void DocModifyEventHandler(ModifyEventArgs info);

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    public sealed class ModifyEventArgs
    {
        /// <summary></summary>
        public ModifyStatus ModifyStatus { get; }

        /// <summary></summary>
        public ModifyDetail ModifyDetail { get; }

        /// <summary></summary>
        public TextRange Range { get; }

        public TextPos OldEndPos { get; }

        /// <summary>
        /// 
        /// </summary>
        internal ModifyEventArgs(ModifyStatus modifyStatus, IHighLevelCommand command, TextPos endPos)
        {
            ModifyStatus = modifyStatus;
            ModifyDetail = command.ModifyDetail;
            Range = command.ResultRange;
            OldEndPos = endPos;
        }
    }
}
