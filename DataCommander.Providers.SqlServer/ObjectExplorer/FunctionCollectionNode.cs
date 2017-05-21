namespace DataCommander.Providers.SqlServer.ObjectExplorer
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal sealed class FunctionCollectionNode : ITreeNode
    {
        public FunctionCollectionNode(DatabaseNode database)
        {
            this.database = database;
        }

        public string Name => "Functions";

        public bool IsLeaf => false;

        IEnumerable<ITreeNode> ITreeNode.GetChildren(bool refresh)
        {
            return new ITreeNode[]
            {
                new TableValuedFunctionCollectionNode(database),
                new ScalarValuedFunctionCollectionNode(database)
            };
        }

        public bool Sortable => false;

        public string Query => null;

        public ContextMenuStrip ContextMenu => null;

        readonly DatabaseNode database;
    }
}