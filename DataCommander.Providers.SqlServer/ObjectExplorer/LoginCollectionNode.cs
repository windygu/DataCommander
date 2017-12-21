﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Foundation.Data;
using Foundation.Data.SqlClient;
using Foundation.Diagnostics.Contracts;

namespace DataCommander.Providers.SqlServer.ObjectExplorer
{
    internal sealed class LoginCollectionNode : ITreeNode
    {
        private readonly ServerNode _server;

        public LoginCollectionNode(ServerNode server)
        {
            FoundationContract.Requires<ArgumentException>(server != null);

            _server = server;
        }

        #region ITreeNode Members

        string ITreeNode.Name => "Logins";
        bool ITreeNode.IsLeaf => false;

        IEnumerable<ITreeNode> ITreeNode.GetChildren(bool refresh)
        {
            const string commandText = @"select name
from sys.server_principals sp (nolock)
where   sp.type in('S','U','G')
order by name";
            var request = new ExecuteReaderRequest(commandText);
            var executor = new SqlCommandExecutor(_server.ConnectionString);
            var loginNodes = executor.ExecuteReader(request, dataRecord => new LoginNode(dataRecord.GetString(0)));
            return loginNodes;
        }

        bool ITreeNode.Sortable => false;
        string ITreeNode.Query => null;
        ContextMenuStrip ITreeNode.ContextMenu => null;

        #endregion
    }
}