﻿using System;
using System.Collections.Generic;
using System.Data;

namespace Foundation.Data
{
    public static class IDbCommandExecutorExtensions
    {
        private static void Execute(this IDbCommandExecutor executor, IEnumerable<ExecuteCommandRequest> requests)
        {
            executor.Execute(connection =>
            {
                foreach (var request in requests)
                    using (var command = connection.CreateCommand(request.CreateCommandRequest))
                        request.Execute(command);
            });
        }

        public static void Execute(this IDbCommandExecutor executor, CreateCommandRequest request, Action<IDbCommand> execute)
        {
            var requests = new[]
            {
                new ExecuteCommandRequest(request, execute)
            };
            executor.Execute(requests);
        }

        public static int ExecuteNonQuery(this IDbCommandExecutor executor, CreateCommandRequest request)
        {
            var affectedRows = 0;
            executor.Execute(request, command => affectedRows = command.ExecuteNonQuery());
            return affectedRows;
        }

        public static object ExecuteScalar(this IDbCommandExecutor executor, CreateCommandRequest request)
        {
            object scalar = null;
            executor.Execute(request, command => scalar = command.ExecuteScalar());
            return scalar;
        }

        public static void ExecuteReader(this IDbCommandExecutor executor, ExecuteReaderRequest request, Action<IDataReader> read)
        {
            executor.Execute(request.CreateCommandRequest, command =>
            {
                using (var dataReader = command.ExecuteReader(request.CommandBehavior))
                    read(dataReader);
            });
        }

        public static List<T> ExecuteReader<T>(this IDbCommandExecutor executor, ExecuteReaderRequest request, Func<IDataRecord, T> read)
        {
            List<T> rows = null;
            executor.ExecuteReader(request, dataReader => rows = dataReader.ReadResult(() => read(dataReader)));
            return rows;
        }

        public static ExecuteReaderResponse<T1, T2> ExecuteReader<T1, T2>(this IDbCommandExecutor executor, ExecuteReaderRequest request,
            Func<IDataRecord, T1> read1, Func<IDataRecord, T2> read2)
        {
            ExecuteReaderResponse<T1, T2> response = null;
            executor.ExecuteReader(request, dataReader => response = dataReader.Read(() => read1(dataReader), () => read2(dataReader)));
            return response;
        }

        public static ExecuteReaderResponse<T1, T2, T3> ExecuteReader<T1, T2, T3>(this IDbCommandExecutor executor, ExecuteReaderRequest request,
            Func<IDataRecord, T1> read1, Func<IDataRecord, T2> read2, Func<IDataRecord, T3> read3)
        {
            ExecuteReaderResponse<T1, T2, T3> response = null;
            executor.ExecuteReader(request,
                dataReader => response = dataReader.Read(() => read1(dataReader), () => read2(dataReader), () => read3(dataReader)));
            return response;
        }

        public static DataTable ExecuteDataTable(this IDbCommandExecutor executor, ExecuteReaderRequest request)
        {
            DataTable dataTable = null;
            executor.Execute(request.CreateCommandRequest, command => { dataTable = command.ExecuteDataTable(request.CancellationToken); });
            return dataTable;
        }

        public static DataSet ExecuteDataSet(this IDbCommandExecutor executor, ExecuteReaderRequest request)
        {
            DataSet dataSet = null;
            executor.Execute(request.CreateCommandRequest, command => { dataSet = command.ExecuteDataSet(request.CancellationToken); });
            return dataSet;
        }
    }
}