﻿using System.Data;
using System.Threading;
using Foundation.Data.SqlClient.SqlLoggedSqlConnection;
using Foundation.Threading;

namespace Foundation.Data.SqlClient
{
    /// <summary>
    /// 
    /// </summary>
    public class SafeLoggedSqlConnectionFactory : IDbConnectionFactory
    {
        private readonly SqlLog.SqlLog _sqlLog;

        private readonly int _applicationId;

        private readonly ISqlLoggedSqlCommandFilter _filter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="applicationName"></param>
        /// <param name="filter"></param>
        public SafeLoggedSqlConnectionFactory(
            string connectionString,
            string applicationName,
            ISqlLoggedSqlCommandFilter filter)
        {
            _filter = filter;
            _sqlLog = new SqlLog.SqlLog(connectionString);
            _applicationId = _sqlLog.ApplicationStart(applicationName, LocalTime.Default.Now, true);
        }

        /// <summary>
        /// 
        /// </summary>
        public WorkerThread Thread => _sqlLog.Thread;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="userName"></param>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public IDbConnection CreateConnection(string connectionString, string userName, string hostName)
        {
            return new SafeLoggedSqlConnection(
                _sqlLog,
                _applicationId,
                userName,
                hostName,
                connectionString,
                _filter,
                CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public IDbConnectionHelper CreateConnectionHelper(IDbConnection connection)
        {
            var safeLoggedSqlConnection = (SafeLoggedSqlConnection)connection;
            var loggedSqlConnection = (SqlLoggedSqlConnection.SqlLoggedSqlConnection)safeLoggedSqlConnection.Connection;
            var sqlConnection = loggedSqlConnection.Connection;
            return new SqlConnectionFactory(sqlConnection, connection);
        }
    }
}