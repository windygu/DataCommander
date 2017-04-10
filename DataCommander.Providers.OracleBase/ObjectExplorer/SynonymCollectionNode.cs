namespace DataCommander.Providers.OracleBase
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Forms;
    using DataCommander.Foundation.Data;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class SynonymCollectionNode : ITreeNode
    {
        public SynonymCollectionNode(SchemaNode schema)
        {
            this.schema = schema;
        }

        public string Name => "Synonyms";

        public bool IsLeaf => false;

        public IEnumerable<ITreeNode> GetChildren(bool refresh)
        {
			var commandText = @"select	s.SYNONYM_NAME
from	SYS.ALL_SYNONYMS s
where	s.OWNER	= '{0}'
order by s.SYNONYM_NAME";
            var transactionScope = new DbTransactionScope(schema.SchemasNode.Connection, null);
            commandText = string.Format(commandText, schema.Name);
            var dataTable = transactionScope.ExecuteDataTable(new CommandDefinition {CommandText = commandText}, CancellationToken.None);
            var count = dataTable.Rows.Count;

            var treeNodes = new ITreeNode[count];

			for (var i = 0; i < count; i++)
			{
				var name = (string) dataTable.Rows[ i ][ 0 ];
				treeNodes[ i ] = new SynonymNode( schema, name );
			}

            return treeNodes;
        }

        public bool Sortable => false;

        public string Query => null;

        public SchemaNode Schema => schema;

        public ContextMenuStrip ContextMenu => null;

        readonly SchemaNode schema;
    }
}