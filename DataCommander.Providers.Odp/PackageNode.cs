namespace DataCommander.Providers.Odp
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Summary description for TablesNode.
    /// </summary>
    internal sealed class PackageNode : ITreeNode
    {
        public PackageNode(
          SchemaNode schema,
          string name)
        {
            this.schemaNode = schema;
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
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
            //string commandText = "select distinct object_name from all_arguments where owner='{0}' and package_name='{1}'";
            //commandText = string.Format(commandText, schema.Name, name);
            string commandText = string.Format(@"select	procedure_name
from	all_procedures
where	owner = '{0}'
	and object_name = '{1}'
order by procedure_name", schemaNode.Name, name);
            List<ITreeNode> list = new List<ITreeNode>();

            using (IDataReader dataReader = schemaNode.SchemasNode.Connection.ExecuteReader(commandText))
            {
                while (dataReader.Read())
                {
                    string procedureName = dataReader.GetString(0);
                    ProcedureNode procedureNode = new ProcedureNode(schemaNode, this, procedureName);
                    list.Add(procedureNode);
                }
            }

            return list.ToArray();
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
                string commandText = "select text from all_source where owner = '{0}' and type = 'PACKAGE' and name = '{1}'";
                commandText = String.Format(commandText, schemaNode.Name, name);
                DataTable dataTable = schemaNode.SchemasNode.Connection.ExecuteDataTable(commandText);
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    DataRow dataRow = dataTable.Rows[i];
                    sb.Append(dataRow[0]);
                }

                return sb.ToString();
            }
        }

        void ScriptPackage(object sender, EventArgs e)
        {
        }

        void ScriptPackageBody(object sender, EventArgs e)
        {
            string commandText = "select text from all_source where owner = '{0}' and name = '{1}' and type = 'PACKAGE BODY'";
            commandText = string.Format(commandText, schemaNode.Name, name);
            DataTable dataTable = schemaNode.SchemasNode.Connection.ExecuteDataTable(commandText);
            DataRowCollection dataRows = dataTable.Rows;
            int count = dataRows.Count;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                DataRow dataRow = dataRows[i];
                string line = (string)dataRow[0];
                sb.Append(line);
            }

            MainForm mainForm = DataCommanderApplication.Instance.MainForm;
            QueryForm queryForm = (QueryForm)mainForm.ActiveMdiChild;
            QueryTextBox tbQuery = queryForm.QueryTextBox;
            int selectionStart = tbQuery.RichTextBox.TextLength;

            string append = sb.ToString();

            tbQuery.RichTextBox.AppendText(append);
            tbQuery.RichTextBox.SelectionStart = selectionStart;
            tbQuery.RichTextBox.SelectionLength = append.Length;

            tbQuery.Focus();
        }

        public ContextMenuStrip ContextMenu
        {
            get
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();

                ToolStripMenuItem menuItemPackage = new ToolStripMenuItem("Script Package", null, ScriptPackage);
                contextMenu.Items.Add(menuItemPackage);

                ToolStripMenuItem menuItemPackageBody = new ToolStripMenuItem("Script Package Body", null, ScriptPackageBody);
                contextMenu.Items.Add(menuItemPackageBody);

                return contextMenu;
            }
        }

        public void BeforeExpand()
        {
        }

        public SchemaNode SchemaNode
        {
            get
            {
                return schemaNode;
            }
        }

        SchemaNode schemaNode;
        string name;
    }
}