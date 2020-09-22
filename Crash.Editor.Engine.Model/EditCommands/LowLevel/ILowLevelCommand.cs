using Crash.Editor.Engine.Model.Common.UndoRedo;

namespace Crash.Editor.Engine.Model.EditCommands.LowLevel
{
    // テキストデータの編集は以下のパターンの組み合わせで処理できる。
    // なので、高レベルコマンド側からは、低レベルコマンドを使用することで任意の編集が可能。
    //
    // 単一行に文字列挿入
    //      Execute時:   行データの指定箇所に文字列を挿入し、ModifyCountを++
    //      Undo時:      挿入した文字列を削除し、ModifyCountを--
    // 単一行で文字列削除
    //      Execute時:   行データの指定範囲を削除してUndo用として保持し、ModifyCountを++
    //      Undo時:      保持していた文字列を挿入し、ModifyCountを--
    // 複数行の挿入
    //      Execute時:   指定箇所に行挿入を行い、挿入した行データのModifyCountを++
    //      Undo時:      挿入した行データを削除し、削除した行データのModifyCountを--
    // 複数行の削除
    //      Execute時:   指定範囲の行データを削除し、Undo用として保持
    //      Undo時:      保持していた行データを挿入

    internal interface ILowLevelCommand : IUndoRedoCommand
    {
    }
}
