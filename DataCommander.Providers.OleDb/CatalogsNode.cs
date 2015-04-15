namespace DataCommander.Providers.OleDb
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Windows.Forms;

    /// <summary>
    /// Summary description for CatalogsNode.
    /// </summary>
    class CatalogsNode : ITreeNode
    {
        public CatalogsNode(OleDbConnection connection)
        {
            this.connection = connection;
        }

        public string Name
        {
            get
            {
                return "Catalogs";
            }
        }

        public bool IsLeaf
        {
            get
            {
                return false;
            }
        }

        public IEnumerable<ITreeNode> GetChildren(bool refresh)
        {
            ITreeNode[] treeNodes;

            try
            {
                DataTable dataTable = this.connection.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
                int count = dataTable.Rows.Count;
                DataColumn nameColumn = dataTable.Columns["CATALOG_NAME"];
                treeNodes = new ITreeNode[count];

                for (int i = 0; i < count; i++)
                {
                    string name = (string)dataTable.Rows[i][nameColumn];
                    treeNodes[i] = new CatalogNode(this.connection, name);
                }
            }
            catch
            {
                treeNodes = new ITreeNode[1];
                treeNodes[0] = new CatalogNode(this.connection, null);
            }

            return treeNodes;
        }

        public bool Sortable
        {
            get
            {
                return false;
            }
        }

        public string Query
        {
            get
            {
                return null;
            }
        }

        public ContextMenuStrip ContextMenu
        {
            get
            {
                return null;
            }
        }

        public void BeforeExpand()
        {
        }

        readonly OleDbConnection connection;
    }
}