namespace DataCommander.Providers.OracleBase
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Forms;
    using DataCommander.Foundation.Data;

    internal class IndexeCollectionNode : ITreeNode
    {
        private readonly TableNode table;

        public IndexeCollectionNode(TableNode tableNode)
        {
            this.table = tableNode;
        }

        public string Name => "Indexes";

        public bool IsLeaf => false;

        public IEnumerable<ITreeNode> GetChildren(bool refresh)
        {
            var commandText = "select index_name from sys.all_indexes where owner = '{0}' and table_name = '{1}' order by index_name";
            commandText = string.Format(commandText, table.Schema.Name, table.Name);
            var command = table.Schema.SchemasNode.Connection.CreateCommand();
            command.CommandText = commandText;
            // TODO
            // command.FetchSize = 256 * 1024;
            var dataTable = command.ExecuteDataTable(CancellationToken.None);
            var count = dataTable.Rows.Count;
            var indexes = new string[count];

            for (var i = 0; i < count; i++)
            {
                var name = (string)dataTable.Rows[i][0];
                indexes[i] = name;
            }

            var treeNodes = new ITreeNode[indexes.Length];

            for (var i = 0; i < indexes.Length; i++)
            {
                treeNodes[i] = new IndexNode(this.table, indexes[i]);
            }

            return treeNodes;
        }

        public bool Sortable => false;

        public string Query => null;

        public ContextMenuStrip ContextMenu => null;
    }
}