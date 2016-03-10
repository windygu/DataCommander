namespace DataCommander.Providers.SqlServer2005.ObjectExplorer
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Windows.Forms;
    using Foundation.Data;

    internal sealed class TableValuedFunctionCollectionNode : ITreeNode
    {
        public TableValuedFunctionCollectionNode(DatabaseNode database)
        {
            this.database = database;
        }

        public string Name => "Table-valued Functions";

        public bool IsLeaf => false;

        IEnumerable<ITreeNode> ITreeNode.GetChildren(bool refresh)
        {
            string commandText = @"select
    s.name	as SchemaName,
	o.name	as Name,
	o.type
from [{0}].sys.schemas s (nolock)
join [{0}].sys.objects o (nolock)
on	s.schema_id = o.schema_id
where o.type in('IF','TF')
order by 1,2";
            commandText = string.Format(commandText, this.database.Name);
            string connectionString = this.database.Databases.Server.ConnectionString;

            return SqlClientFactory.Instance.ExecuteReader(
                this.database.Databases.Server.ConnectionString,
                new CommandDefinition {CommandText = commandText},
                CommandBehavior.Default,
                dataRecord =>
                {
                    string owner = dataRecord.GetString(0);
                    string name = dataRecord.GetString(1);
                    string xtype = dataRecord.GetString(2);
                    return new FunctionNode(this.database, owner, name, xtype);
                });
        }

        public bool Sortable => false;

        public string Query => null;

        public ContextMenuStrip ContextMenu => null;

        readonly DatabaseNode database;
    }
}