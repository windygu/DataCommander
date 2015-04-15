﻿namespace DataCommander.Providers.Msi
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using Microsoft.Deployment.WindowsInstaller;

    internal sealed class MsiConnection : IDbConnection
    {
        #region Private Fields

        private readonly string connectionString;
        private Database database;
        private ConnectionState state;

        #endregion

        public MsiConnection(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Open()
        {
            var sb = new DbConnectionStringBuilder();
            sb.ConnectionString = this.connectionString;
            Contract.Assert(sb.ContainsKey("Data Source"));
            object dataSourceObject = sb["Data Source"];
            Contract.Assert(dataSourceObject is string);
            string path = (string)dataSourceObject;
            this.database = new Database(path, DatabaseOpenMode.ReadOnly);
            this.state = ConnectionState.Open;
        }

        public MsiCommand CreateCommand()
        {
            return new MsiCommand(this);
        }

        internal Database Database
        {
            get
            {
                return this.database;
            }
        }

        #region IDbConnection Members

        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        IDbTransaction IDbConnection.BeginTransaction()
        {
            throw new NotImplementedException();
        }

        void IDbConnection.ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        void IDbConnection.Close()
        {
            this.database.Close();
        }

        string IDbConnection.ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int IDbConnection.ConnectionTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            throw new NotImplementedException();
        }

        string IDbConnection.Database
        {
            get
            {
                return this.database.FilePath;
            }
        }

        void IDbConnection.Open()
        {
            throw new NotImplementedException();
        }

        ConnectionState IDbConnection.State
        {
            get
            {
                return this.state;
            }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            database.Dispose();
        }

        #endregion
    }
}