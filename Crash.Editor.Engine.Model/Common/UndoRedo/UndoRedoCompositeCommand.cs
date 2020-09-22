using System.Collections.Generic;
using System.Linq;
using Crash.Core.Collections;

namespace Crash.Editor.Engine.Model.Common.UndoRedo
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UndoRedoCompositeCommand<TCommand> : IUndoRedoCommand
        where TCommand : class, IUndoRedoCommand
    {
        private readonly List<TCommand> _commandList = new List<TCommand>();

        private TCommand? _lastInvokeCommand;

        public TCommand LastInvokeCommand => _lastInvokeCommand!;

        public bool Has => _commandList.Any();

        public bool IsEmpty => !Has;

        public IEnumerable<TCommand> Commands => _commandList;

        public void Add(TCommand command)
        {
            _commandList.Add(command);
        }

        public void Clear()
        {
            _commandList.Clear();
        }

        public void Execute()
        {
            foreach (var command in _commandList)
            {
                command.Execute();
                _lastInvokeCommand = command;
            }
        }

        public void Undo()
        {
            foreach (var command in _commandList.ReverseList())
            {
                command.Undo();
                _lastInvokeCommand = command;
            }
        }
    }
}
