﻿using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DataCommander.Providers.Connection;
using Npgsql;

namespace DataCommander.Providers.PostgreSql
{
    internal sealed class Connection : ConnectionBase
    {
        #region Private Fields

        private readonly string _connectionString;
        private readonly NpgsqlConnection _npgsqlConnection;

        #endregion

        public Connection(string connectionString)
        {
            _connectionString = connectionString;
            _npgsqlConnection = new NpgsqlConnection(connectionString);
            Connection = _npgsqlConnection;
        }

        public override Task OpenAsync(CancellationToken cancellationToken) => _npgsqlConnection.OpenAsync(cancellationToken);
        public override IDbCommand CreateCommand() => _npgsqlConnection.CreateCommand();
        public override string ConnectionName { get; set; }
        public override string Caption => _npgsqlConnection.Database;
        public override string DataSource => _npgsqlConnection.DataSource;
        protected override void SetDatabase(string database) => throw new NotImplementedException();
        public override string ServerVersion => _npgsqlConnection.ServerVersion;
        public override int TransactionCount => 0;
    }
}