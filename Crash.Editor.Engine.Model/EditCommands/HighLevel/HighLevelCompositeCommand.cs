using Crash.Editor.Engine.Model.Common.UndoRedo;

namespace Crash.Editor.Engine.Model.EditCommands.HighLevel
{
    internal sealed class HighLevelCompositeCommand : IHighLevelCommand
    {
        private readonly UndoRedoCompositeCommand<IHighLevelCommand> _composite = new UndoRedoCompositeCommand<IHighLevelCommand>();

        public ModifyDetail ModifyDetail => _composite.LastInvokeCommand.ModifyDetail;

        public TextRange ResultRange => _composite.LastInvokeCommand.ResultRange;

        public void TryAdd(IHighLevelCommand command)
        {
            if (command.Validate())
            {
                _composite.Add(command);
            }
        }

        public bool Validate()
        {
            return _composite.Has;
        }

        public void Execute()
        {
            _composite.Execute();
        }

        public void Undo()
        {
            _composite.Undo();
        }
    }
}
