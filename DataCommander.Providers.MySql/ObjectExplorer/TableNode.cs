﻿namespace DataCommander.Providers.MySql.ObjectExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Foundation.Data;
    using global::MySql.Data.MySqlClient;

    internal sealed class TableNode : ITreeNode
    {
        private readonly DatabaseNode databaseNode;
        private readonly string name;

        public TableNode(DatabaseNode databaseNode, string name)
        {
            this.databaseNode = databaseNode;
            this.name = name;
        }

        string ITreeNode.Name => this.name;

        bool ITreeNode.IsLeaf => true;

        IEnumerable<ITreeNode> ITreeNode.GetChildren(bool refresh)
        {
            throw new NotImplementedException();
        }

        bool ITreeNode.Sortable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        string ITreeNode.Query => $@"select *
from {this.databaseNode.Name}.{this.name}";

        System.Windows.Forms.ContextMenuStrip ITreeNode.ContextMenu
        {
            get
            {
                var menu = new ContextMenuStrip();

                var item = new ToolStripMenuItem("Show create table", null, this.ShowCreateTable_Click);
                menu.Items.Add(item);

                return menu;
            }
        }

        private void ShowCreateTable_Click(object sender, EventArgs e)
        {
            string commandText = $"show create table {this.databaseNode.Name}.{this.name}";
            var createTableStatement = MySqlClientFactory.Instance.ExecuteReader(
                this.databaseNode.ObjectExplorer.ConnectionString,
                new CommandDefinition {CommandText = commandText},
                CommandBehavior.Default,
                dataRecord => dataRecord.GetString(1)).First();

            Clipboard.SetText(createTableStatement);
            var queryForm = (QueryForm)DataCommanderApplication.Instance.MainForm.ActiveMdiChild;
            queryForm.SetStatusbarPanelText("Copying create table statement to clipboard finished.", SystemColors.ControlText);
        }
    }
}