using System;
using System.Collections.Generic;
using System.Linq;

namespace Crash.Core.Collections
{
    /// <summary>
    /// ツリー構造を委譲によって提供する。
    /// </summary>
    public sealed class TreeNodeContext<TNode>
        where TNode : class
    {
        /// <summary></summary>
        private readonly Func<TNode, TreeNodeContext<TNode>> _tncGetter;

        /// <summary></summary>
        private readonly TNode _node;

        /// <summary></summary>
        private readonly List<TNode> _children = new List<TNode>();

        /// <summary></summary>
        public TNode? Parent { get; private set; }

        /// <summary></summary>
        public bool IsTop => Parent == null;

        /// <summary></summary>
        public IReadOnlyList<TNode> Children => _children;

        /// <summary></summary>
        public bool IsRoot { get; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        private TreeNodeContext(bool isRoot, TNode node, Func<TNode, TreeNodeContext<TNode>> tncGetter)
        {
            IsRoot = isRoot;
            _node = node;
            _tncGetter = tncGetter;
        }

        /// <summary>
        /// ルートノード用コンテキストを生成する。
        /// </summary>
        public static TreeNodeContext<TNode> CreateForRootNode(TNode node, Func<TNode, TreeNodeContext<TNode>> tncGetter)
        {
            return new TreeNodeContext<TNode>(true, node, tncGetter);
        }

        /// <summary>
        /// 通常ノード用コンテキストを生成する。
        /// </summary>
        public static TreeNodeContext<TNode> CreateForNormalNode(TNode node, Func<TNode, TreeNodeContext<TNode>> tncGetter)
        {
            return new TreeNodeContext<TNode>(false, node, tncGetter);
        }

        /// <summary>
        /// 子ノードを追加する。
        /// </summary>
        public void AddNode(TNode child)
        {
            InsertNode(_children.Count, child);
        }

        /// <summary>
        /// 子ノードを挿入する。
        /// </summary>
        public void InsertNode(int index, TNode child)
        {
            var childTnc = _tncGetter(child);
            Verifier.Verify<ArgumentException>(!childTnc.IsRoot);
            Verifier.Verify<ArgumentException>(childTnc.Parent == null);

            childTnc.Parent = _node;
            _children.Insert(index, child);
        }

        /// <summary>
        /// 子ノードを削除する。
        /// </summary>
        public void RemoveNode(TNode child)
        {
            var childTnc = _tncGetter(child);
            Verifier.Verify<ArgumentException>(object.ReferenceEquals(childTnc.Parent, _node));

            childTnc.Parent = null;
            _children.Remove(child);
        }

        /// <summary>
        /// ルートノードを検索する。自分がルートノードの場合は自分を返す。
        /// </summary>
        public TNode? FindRoot()
        {
            var node = GetTop();
            return _tncGetter(node).IsRoot ? node : null;
        }

        /// <summary>
        /// ルートノードを取得する。自分がルートノードの場合は自分を返す。
        /// </summary>
        public TNode GetRoot()
        {
            return FindRoot()!;
        }

        /// <summary>
        /// トップノードを取得する。自分がトップノードの場合は自分を返す。
        /// </summary>
        public TNode GetTop()
        {
            return GetParentsAndSelf().Last();
        }

        /// <summary>
        /// 親ノードを取得する。
        /// </summary>
        public TNode GetParent()
        {
            return Parent!;
        }

        /// <summary>
        /// 全ての親ノードをボトムアップで列挙する。
        /// </summary>
        public IEnumerable<TNode> GetParents()
        {
            return GetParentsAndSelf().Skip(1);
        }

        /// <summary>
        /// 自分及び全ての親ノードをボトムアップで列挙する。
        /// </summary>
        public IEnumerable<TNode> GetParentsAndSelf()
        {
            return _node.GetParentsAndSelf(true, node => _tncGetter(node).Parent);
        }

        /// <summary>
        /// 指定型の親ノードをボトムアップで検索する。
        /// </summary>
        public TParent? FindParent<TParent>()
            where TParent : class, TNode
        {
            return GetParents().OfType<TParent>().FirstOrNull();
        }

        /// <summary>
        /// 指定型の親ノードをボトムアップで取得する。
        /// </summary>
        public TParent GetParent<TParent>()
            where TParent : class, TNode
        {
            return FindParent<TParent>()!;
        }

        /// <summary>
        /// 指定型の子ノードをトップダウンで検索する。
        /// </summary>
        public TChild? FindChild<TChild>()
            where TChild : class, TNode
        {
            return GetAllChildren().OfType<TChild>().FirstOrNull();
        }

        /// <summary>
        /// 指定型の子ノードをトップダウンで取得する。
        /// </summary>
        public TChild GetChild<TChild>()
            where TChild : class, TNode
        {
            return FindChild<TChild>()!;
        }

        /// <summary>
        /// 自分及び全ての子ノードをトップダウンで列挙する。
        /// </summary>
        public IEnumerable<TNode> GetAllChildrenAndSelf()
        {
            yield return _node;

            foreach (var child in _children.SelectMany(child => _tncGetter(child).GetAllChildrenAndSelf()))
            {
                yield return child;
            }
        }

        /// <summary>
        /// 自分及び全ての子ノードをボトムアップで列挙する。
        /// </summary>
        public IEnumerable<TNode> GetAllChildrenAndSelfReverse()
        {
            foreach (var child in _children.ReverseList().SelectMany(child => _tncGetter(child).GetAllChildrenAndSelfReverse()))
            {
                yield return child;
            }

            yield return _node;
        }

        /// <summary>
        /// 全ての子ノードをトップダウンで列挙する。
        /// </summary>
        public IEnumerable<TNode> GetAllChildren()
        {
            return GetAllChildrenAndSelf().Where(element => !object.ReferenceEquals(element, _node));
        }

        /// <summary>
        /// 全ての子ノードをボトムアップで列挙する。
        /// </summary>
        public IEnumerable<TNode> GetAllChildrenReverse()
        {
            return GetAllChildrenAndSelfReverse().Where(element => !object.ReferenceEquals(element, _node));
        }
    }
}
