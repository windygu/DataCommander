﻿using System.Collections.Generic;
using System.Windows.Forms;

namespace DataCommander.Providers.OracleBase.ObjectExplorer
{
    /// <summary>
    /// Summary description for TablesNode.
    /// </summary>
    public sealed class TriggerNode : ITreeNode
    {
		private readonly TableNode _tableNode;
		private readonly string _name;

        public TriggerNode(
          TableNode tableNode,
          string name)
        {
            _tableNode = tableNode;
            _name = name;
        }

        public string Name => _name;

        public bool IsLeaf => true;

        public IEnumerable<ITreeNode> GetChildren(bool refresh)
        {
            return null;
        }

        public bool Sortable => false;

        public string Query => "select * from " + _tableNode.Schema.Name + "." + _name;

        public ContextMenuStrip ContextMenu => null;
    }
}