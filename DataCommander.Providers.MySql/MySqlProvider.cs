﻿namespace DataCommander.Providers.MySql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DataCommander.Foundation.Configuration;
    using DataCommander.Foundation.Data;
    using DataCommander.Foundation.Diagnostics;
    using DataCommander.Providers;
    using global::MySql.Data.MySqlClient;

    internal sealed class MySqlProvider : IProvider
    {
        #region Private Fields

        private static readonly ILog log = LogFactory.Instance.GetCurrentTypeLog();
        private static string[] keyWords;
        private ObjectExplorer objectExplorer;

        #endregion

        #region IProvider Members

        string IProvider.Name
        {
            get
            {
                return "MySql";
            }
        }

        DbProviderFactory IProvider.DbProviderFactory
        {
            get
            {
                return MySqlClientFactory.Instance;
            }
        }

        string[] IProvider.KeyWords
        {
            get
            {
                if (keyWords == null)
                {
                    ConfigurationNode folder = Settings.CurrentType;
                    keyWords = folder.Attributes["MySqlKeyWords"].GetValue<string[]>();
                }

                return keyWords;
            }
        }

        bool IProvider.CanConvertCommandToString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IProvider.IsCommandCancelable
        {
            get
            {
                return true;
            }
        }

        IObjectExplorer IProvider.ObjectExplorer
        {
            get
            {
                if (this.objectExplorer == null)
                {
                    this.objectExplorer = new ObjectExplorer();
                }

                return this.objectExplorer;
            }
        }

        void IProvider.ClearCompletionCache()
        {
            throw new NotImplementedException();
        }

        string IProvider.CommandToString(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        ConnectionBase IProvider.CreateConnection(string connectionString)
        {
            return new Connection(connectionString);
        }

        DbDataAdapter IProvider.CreateDataAdapter(string selectCommandText, IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        IDataReaderHelper IProvider.CreateDataReaderHelper(IDataReader dataReader)
        {
            return new MySqlDataReaderHelper((MySqlDataReader)dataReader);
        }

        void IProvider.CreateInsertCommand(DataTable sourceSchemaTable, string[] sourceDataTypeNames, IDbConnection destinationconnection,
            string destinationTableName, out IDbCommand insertCommand, out Converter<object, object>[] converters)
        {
            throw new NotImplementedException();
        }

        void IProvider.DeriveParameters(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        System.Xml.XmlReader IProvider.ExecuteXmlReader(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        Type IProvider.GetColumnType(Foundation.Data.DataColumnSchema dataColumnSchema)
        {
            // TODO

            //var dbType = (MySqlDbType)dataColumnSchema.ProviderType;
            //int columnSize = dataColumnSchema.ColumnSize;
            //Type type;

            return typeof (object);
        }

        string IProvider.GetColumnTypeName(IProvider sourceProvider, DataRow sourceSchemaRow, string sourceDataTypeName)
        {
            throw new NotImplementedException();
        }

        GetCompletionResponse IProvider.GetCompletion(ConnectionBase connection, IDbTransaction transaction, string text, int position)
        {
            var response = new GetCompletionResponse
            {
                FromCache = false
            };

            var sqlStatement = new SqlStatement(text);
            Token[] tokens = sqlStatement.Tokens;
            Token previousToken, currentToken;
            sqlStatement.FindToken(position, out previousToken, out currentToken);

            if (currentToken != null)
            {
                var parts = new IdentifierParser(new StringReader(currentToken.Value)).Parse();
                var lastPart = parts.Last();
                int lastPartLength = lastPart != null ? lastPart.Length : 0;
                response.StartPosition = currentToken.EndPosition - lastPartLength + 1;
                response.Length = lastPartLength;
            }
            else
            {
                response.StartPosition = position;
                response.Length = 0;
            }

            var sqlObject = sqlStatement.FindSqlObject(previousToken, currentToken);
            if (sqlObject != null)
            {
                var statements = new List<string>();

                switch (sqlObject.Type)
                {
                    case SqlObjectTypes.Database:
                        statements.Add(SqlServerObject.GetDatabases());
                        break;

                    case SqlObjectTypes.Table | SqlObjectTypes.View | SqlObjectTypes.Function:
                    {
                        var nameParts = new IdentifierParser(new StringReader(sqlObject.Name ?? string.Empty)).Parse().ToList();
                        var name = new DatabaseObjectMultipartName(connection.Database, nameParts);

                        switch (nameParts.Count)
                        {
                            case 0:
                            case 1:
                                statements.Add(SqlServerObject.GetDatabases());
                                statements.Add(SqlServerObject.GetTables(name.Database, new string[] {"BASE TABLE", "SYSTEM VIEW"}));
                                break;

                            case 2:
                                statements.Add(SqlServerObject.GetTables(name.Database, new string[] {"BASE TABLE", "SYSTEM VIEW"}));
                                break;
                        }
                    }
                        break;

                    case SqlObjectTypes.Column:
                    {
                        var nameParts = new IdentifierParser(new StringReader(sqlObject.ParentName ?? string.Empty)).Parse().ToList();
                        var name = new DatabaseObjectMultipartName(connection.Database, nameParts);
                        statements.Add(SqlServerObject.GetColumns(name.Database, name.Name));
                    }
                        break;
                }

                var objectNames = new List<IObjectName>();

                foreach (string statement in statements)
                {
                    using (var context = connection.Connection.ExecuteReader(null, statement, CommandType.Text, 0, CommandBehavior.Default))
                    {
                        var dataReader = context.DataReader;
                        while (dataReader.Read())
                        {
                            string objectName = dataReader.GetString(0);
                            objectNames.Add(new ObjectName(null, objectName));
                        }
                    }
                }

                response.Items = objectNames;
            }

            return response;
        }

        DataParameterBase IProvider.GetDataParameter(IDataParameter parameter)
        {
            throw new NotImplementedException();
        }

        string IProvider.GetExceptionMessage(Exception exception)
        {
            string message;
            var mySqlException = exception as MySqlException;

            if (mySqlException != null)
            {
                message = string.Format("ErrorCode: {0}, Number: {1}, Message: {2}", mySqlException.ErrorCode, mySqlException.Number, mySqlException.Message);
            }
            else
            {
                message = exception.ToString();
            }

            return message;
        }

        DataTable IProvider.GetParameterTable(IDataParameterCollection parameters)
        {
            throw new NotImplementedException();
        }

        DataTable IProvider.GetSchemaTable(IDataReader dataReader)
        {
            DataTable table = null;
            DataTable schemaTable = dataReader.GetSchemaTable();

            if (schemaTable != null)
            {
                log.Trace("\r\n" + schemaTable.ToStringTable().ToString());

                table = new DataTable("SchemaTable");
                var columns = table.Columns;
                columns.Add(" ", typeof (int));
                columns.Add("  ", typeof (string));
                columns.Add("Name", typeof (string));
                columns.Add("Size", typeof (int));
                columns.Add("DbType", typeof (string));
                columns.Add("DataType", typeof (Type));
                int columnIndex = 0;
                int? columnOrdinalAddition = null;

                foreach (DataRow dataRow in schemaTable.Rows)
                {
                    var dataColumnSchema = new DataColumnSchema(dataRow);
                    int columnOrdinal = dataColumnSchema.ColumnOrdinal;

                    if (columnOrdinalAddition == null)
                    {
                        if (columnOrdinal == 0)
                        {
                            columnOrdinalAddition = 1;
                        }
                        else
                        {
                            columnOrdinalAddition = 0;
                        }
                    }

                    string pk = string.Empty;

                    if (dataColumnSchema.IsKey == true)
                    {
                        pk = "PKEY";
                    }

                    if (dataColumnSchema.IsIdentity == true)
                    {
                        if (pk.Length > 0)
                        {
                            pk += ',';
                        }

                        pk += "IDENTITY";
                    }

                    int columnSize = dataColumnSchema.ColumnSize;
                    var dbType = (MySqlDbType)dataColumnSchema.ProviderType;
                    string dataTypeName = dataReader.GetDataTypeName(columnIndex).ToLowerInvariant();
                    var sb = new StringBuilder();
                    sb.Append(dataTypeName);

                    switch (dbType)
                    {
                        case MySqlDbType.VarChar:
                        case MySqlDbType.Binary:
                        case MySqlDbType.VarBinary:
                        case MySqlDbType.String: // CHAR(n), enum
                            string columnSizeString;

                            if (columnSize == int.MaxValue)
                            {
                                columnSizeString = "max";
                            }
                            else
                            {
                                columnSizeString = columnSize.ToString();
                            }

                            sb.AppendFormat("({0})", columnSizeString);
                            break;

                        case MySqlDbType.Decimal:
                            short precision = dataColumnSchema.NumericPrecision.GetValueOrDefault();
                            short scale = dataColumnSchema.NumericScale.GetValueOrDefault();

                            if (scale == 0)
                                sb.AppendFormat("({0})", precision);
                            else
                                sb.AppendFormat("({0},{1})", precision, scale);

                            break;

                        case MySqlDbType.Byte:
                        case MySqlDbType.Int16:
                        case MySqlDbType.Int24:
                        case MySqlDbType.Int32:
                        case MySqlDbType.Int64:
                            sb.AppendFormat("({0})", columnSize);
                            break;

                        case MySqlDbType.UByte:
                        case MySqlDbType.UInt16:
                        case MySqlDbType.UInt24:
                        case MySqlDbType.UInt32:
                        case MySqlDbType.UInt64:
                            sb.AppendFormat("({0}) unsigned", columnSize);
                            break;

                        case MySqlDbType.Date:
                            break;

                        default:
                            break;
                    }

                    bool allowDBNull = dataColumnSchema.AllowDBNull.GetValueOrDefault();
                    if (!allowDBNull)
                    {
                        sb.Append(" not null");
                    }

                    table.Rows.Add(new object[]
                    {
                        columnOrdinal + columnOrdinalAddition,
                        pk,
                        dataColumnSchema.ColumnName,
                        columnSize,
                        sb.ToString(),
                        dataColumnSchema.DataType
                    });

                    columnIndex++;
                }
            }

            return table;
        }

        List<string> IProvider.GetStatements(string commandText)
        {
            return new List<string>
            {
                commandText
            };
        }

        DataSet IProvider.GetTableSchema(IDbConnection connection, string tableName)
        {
            throw new NotImplementedException();
        }

        List<InfoMessage> IProvider.ToInfoMessages(Exception e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}