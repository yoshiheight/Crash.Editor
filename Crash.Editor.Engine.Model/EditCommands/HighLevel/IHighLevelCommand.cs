using Crash.Editor.Engine.Model.Common.UndoRedo;

namespace Crash.Editor.Engine.Model.EditCommands.HighLevel
{
    internal interface IHighLevelCommand : IUndoRedoCommand
    {
        ModifyDetail ModifyDetail { get; }

        TextRange ResultRange { get; }

        bool Validate();
    }
}
