namespace Crash.Editor.Engine.Model.Common.UndoRedo
{
    /// <summary>
    /// 実行処理用インターフェイス。
    /// 差分保持＋編集処理による実装にしてもいいし、全体のスナップショットを保持する実装にしてもいい。
    /// </summary>
    public interface IUndoRedoCommand
    {
        void Execute();

        void Undo();
    }
}
