namespace DataCommander.Foundation.Data.SqlClient.SqlLoggedSqlConnection
{
    using System;
    using System.Data;

    internal sealed class SqlLoggedSqlCommand : IDbCommand
    {
        private readonly SqlLoggedSqlConnection connection;
        private readonly IDbCommand command;

        public SqlLoggedSqlCommand(
            SqlLoggedSqlConnection connection,
            IDbCommand command)
        {
#if CONTRACTS_FULL
            Contract.Requires(connection != null);
            Contract.Requires(command != null);
#endif

            this.connection = connection;
            this.command = command;
        }

        public string CommandText
        {
            get => this.command.CommandText;

            set => this.command.CommandText = value;
        }

        public int CommandTimeout
        {
            get => this.command.CommandTimeout;

            set => this.command.CommandTimeout = value;
        }

        public CommandType CommandType
        {
            get => this.command.CommandType;

            set => this.command.CommandType = value;
        }

        public IDbConnection Connection
        {
            get => this.connection;

            set => throw new NotImplementedException();
        }

        public IDataParameterCollection Parameters => this.command.Parameters;

        public IDbTransaction Transaction
        {
            get => this.command.Transaction;

            set => this.command.Transaction = value;
        }

        public UpdateRowSource UpdatedRowSource
        {
            get => this.command.UpdatedRowSource;

            set => this.command.UpdatedRowSource = value;
        }

        public void Dispose()
        {
            this.command.Dispose();
        }

        public void Cancel()
        {
            this.command.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return this.command.CreateParameter();
        }

        public int ExecuteNonQuery()
        {
            return this.connection.ExecuteNonQuery(this.command);
        }

        public IDataReader ExecuteReader()
        {
            var loggedSqlDataReader = new SqlLoggedSqlDataReader(this.connection, this.command);
            return loggedSqlDataReader.Execute();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var loggedSqlDataReader = new SqlLoggedSqlDataReader(this.connection, this.command);
            return loggedSqlDataReader.Execute(behavior);
        }

        public object ExecuteScalar()
        {
            return this.connection.ExecuteScalar(this.command);
        }

        public void Prepare()
        {
            this.command.Prepare();
        }
    }
}