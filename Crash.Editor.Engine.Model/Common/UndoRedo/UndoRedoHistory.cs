using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Crash.Editor.Engine.Model.Common.UndoRedo
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UndoRedoHistory<TCommand>
        where TCommand : class, IUndoRedoCommand
    {
        private readonly Stack<TCommand> _undoStack = new Stack<TCommand>();

        private readonly Stack<TCommand> _redoStack = new Stack<TCommand>();

        public bool CanUndo => _undoStack.Any();

        public bool CanRedo => _redoStack.Any();

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void TrimExcess()
        {
            _undoStack.TrimExcess();
            _redoStack.TrimExcess();
        }

        public void Execute(TCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public bool TryUndo([MaybeNullWhen(false)] out TCommand outCommand)
        {
            outCommand = null!;
            if (CanUndo)
            {
                outCommand = _undoStack.Pop();
                outCommand.Undo();
                _redoStack.Push(outCommand);
                return true;
            }
            return false;
        }

        public bool TryRedo([MaybeNullWhen(false)] out TCommand outCommand)
        {
            outCommand = null!;
            if (CanRedo)
            {
                outCommand = _redoStack.Pop();
                outCommand.Execute();
                _undoStack.Push(outCommand);
                return true;
            }
            return false;
        }
    }
}
