namespace DataCommander.Providers.SqlServer2005.ObjectExplorer
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Windows.Forms;
    using Foundation.Data;

    internal sealed class TableCollectionNode : ITreeNode
    {
        public TableCollectionNode( DatabaseNode databaseNode )
        {
            this.DatabaseNode = databaseNode;
        }

        public string Name => "Tables";

        public bool IsLeaf => false;

        IEnumerable<ITreeNode> ITreeNode.GetChildren( bool refresh )
        {
//            string commandText = @"select	u.name,o.name
//from	{0}.dbo.sysobjects o
//join	{0}.dbo.sysusers u
//on	o.uid	= u.uid
//where	o.type='{1}'
//order by u.name,o.name";
//            string databaseName = databaseNode.Name;
//            commandText = string.Format( commandText, databaseName, type );
//            DataTable dataTable = new DataTable();
//            SqlDatabase.Fill( commandText, databaseNode.Connection, dataTable );
//            int count = dataTable.Rows.Count;
//            ITreeNode[] treeNodes = new ITreeNode[ count ];

//            for (int i = 0; i < count; i++)
//            {
//                DataRow row = dataTable.Rows[ i ];
//                string owner = (string) row[ 0 ];
//                string name = (string) row[ 1 ];
//                treeNodes[ i ] = new TableNode( databaseNode, owner, name );
//            }

//            return treeNodes;

            var childNodes = new List<ITreeNode>();
            childNodes.Add( new SystemTableCollectionNode( this.DatabaseNode ) );

            var commandText = string.Format(@"select
    s.name as [Schema],
    tbl.name AS [Name]
from [{0}].sys.tables as tbl (nolock)
join [{0}].sys.schemas s (nolock)
    on tbl.schema_id = s.schema_id
where
(CAST(
 case 
    when tbl.is_ms_shipped = 1 then 1
    when (
        select 
            major_id 
        from 
            [{0}].sys.extended_properties 
        where 
            major_id = tbl.object_id and 
            minor_id = 0 and 
            class = 1 and 
            name = N'microsoft_database_tools_support') 
        is not null then 1
    else 0
end          
             AS bit)=0)
order by
[Schema] ASC,[Name] ASC", this.DatabaseNode.Name);
            var connectionString = this.DatabaseNode.Databases.Server.ConnectionString;
            DataTable dataTable;
            using (var connection = new SqlConnection(connectionString))
            {
                var transactionScope = new DbTransactionScope(connection, null);
                dataTable = transactionScope.ExecuteDataTable(new CommandDefinition { CommandText = commandText }, CancellationToken.None);
            }
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var schema = (string) dataRow[ "Schema" ];
                var name = (string) dataRow[ "Name" ];
                var tableNode = new TableNode( this.DatabaseNode, schema, name );
                childNodes.Add( tableNode );
            }
            return childNodes;
        }

        public bool Sortable => false;

        public string Query => null;

        public DatabaseNode DatabaseNode { get; }

        public ContextMenuStrip ContextMenu => null;
    }
}