﻿namespace DataCommander.Providers.PostgreSql
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using DataCommander.Providers;
    using Npgsql;

    internal sealed class Connection : ConnectionBase
    {
        #region Private Fields

        private readonly string connectionString;
        private readonly NpgsqlConnection npgsqlConnection;
        private string connectionName;

        #endregion

        public Connection(string connectionString)
        {
            this.connectionString = connectionString;
            this.npgsqlConnection = new NpgsqlConnection(connectionString);
            this.Connection = this.npgsqlConnection;
        }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return this.npgsqlConnection.OpenAsync(cancellationToken);
        }

        public override IDbCommand CreateCommand()
        {
            return this.npgsqlConnection.CreateCommand();
        }

        public override string ConnectionName
        {
            get
            {
                return this.connectionName;
            }

            set
            {
                this.connectionName = value;
            }
        }

        public override string Caption
        {
            get
            {
                return this.npgsqlConnection.Database;
            }
        }

        public override string DataSource
        {
            get
            {
                return this.npgsqlConnection.DataSource;
            }
        }

        protected override void SetDatabase(string database)
        {
            throw new NotImplementedException();
        }

        public override string ServerVersion
        {
            get
            {
                return this.npgsqlConnection.ServerVersion;
            }
        }

        public override int TransactionCount
        {
            get
            {
                // TODO
                return 0;
            }
        }
    }
}