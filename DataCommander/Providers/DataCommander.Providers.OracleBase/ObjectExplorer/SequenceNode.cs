﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataCommander.Providers.OracleBase.ObjectExplorer
{
    public sealed class SequenceNode : ITreeNode
	{
		private SchemaNode _schemaNode;
		private readonly string _name;

		public SequenceNode( SchemaNode schemaNode, string name )
		{
			_schemaNode = schemaNode;
			_name = name;
		}

		#region ITreeNode Members

		string ITreeNode.Name => _name;

        bool ITreeNode.IsLeaf => true;

        IEnumerable<ITreeNode> ITreeNode.GetChildren( bool refresh )
		{
			throw new NotImplementedException();
		}

		bool ITreeNode.Sortable => throw new NotImplementedException();

	    string ITreeNode.Query => throw new NotImplementedException();

	    ContextMenuStrip ITreeNode.ContextMenu => throw new NotImplementedException();

	    #endregion
	}
}
