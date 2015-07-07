namespace DataCommander.Providers.SqlServer2005
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using DataCommander.Foundation;
    using DataCommander.Foundation.Configuration;
    using DataCommander.Foundation.Data;
    using DataCommander.Foundation.Data.SqlClient;
    using DataCommander.Foundation.Diagnostics;
    using DataCommander.Foundation.Linq;

    internal sealed class SqlServerProvider : IProvider
    {
        #region Private Fields

        private static readonly ILog log = LogFactory.Instance.GetCurrentTypeLog();
        private ObjectExplorer objectBrowser;
        private static string[] keyWords;
        private static readonly int shortStringSize;

        #endregion

        #region Constructors

        static SqlServerProvider()
        {
            var configurationNode = Settings.CurrentType;
            shortStringSize = configurationNode.Attributes["ShortStringSize"].GetValue<int>();
        }

        #endregion

        public static int ShortStringSize
        {
            get
            {
                return shortStringSize;
            }
        }

        #region IProvider Members

        #region Properties

        string IProvider.Name
        {
            get
            {
                return "SqlServer2005";
            }
        }

        DbProviderFactory IProvider.DbProviderFactory
        {
            get
            {
                return SqlClientFactory.Instance;
            }
        }

        ConnectionBase IProvider.CreateConnection(string connectionString)
        {
            return new Connection(connectionString);
        }

        string[] IProvider.KeyWords
        {
            get
            {
                if (keyWords == null)
                {
                    ConfigurationNode folder = Settings.CurrentType;
                    keyWords = folder.Attributes["TSqlKeyWords"].GetValue<string[]>();
                }

                return keyWords;
            }
        }

        bool IProvider.CanConvertCommandToString
        {
            get
            {
                return true;
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
                if (this.objectBrowser == null)
                {
                    this.objectBrowser = new ObjectExplorer();
                }

                return this.objectBrowser;
            }
        }

        #endregion

        #region Methods

        public void ClearCompletionCache()
        {
        }

        string IProvider.CommandToString(IDbCommand command)
        {
            var sqlCommand = (SqlCommand)command;
            return sqlCommand.ToLogString();
        }

        IDataReaderHelper IProvider.CreateDataReaderHelper(IDataReader dataReader)
        {
            var sqlDataReaderHelper = new SqlDataReaderHelper(dataReader);
            return sqlDataReaderHelper;
        }

        public DbDataAdapter CreateDataAdapter(string selectCommandText, IDbConnection connection)
        {
            return null;
        }

        void IProvider.CreateInsertCommand(
            DataTable sourceSchemaTable,
            string[] sourceDataTypeNames,
            IDbConnection destinationconnection,
            string destinationTableName,
            out IDbCommand insertCommand,
            out Converter<object, object>[] converters)
        {
            DataTable schemaTable;
            string[] dataTypeNames;
            int count;

            var sourceColumnNames =
                (from sourceSchemaRow in sourceSchemaTable.AsEnumerable()
                    select new DataColumnSchema(sourceSchemaRow).ColumnName);

            using (IDbCommand command = destinationconnection.CreateCommand())
            {
                command.CommandText = string.Format("select {0} from {1}", string.Join(",", sourceColumnNames), destinationTableName);
                command.CommandType = CommandType.Text;

                using (IDataReader dataReader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    schemaTable = dataReader.GetSchemaTable();
                    count = dataReader.FieldCount;
                    dataTypeNames = new string[count];

                    for (int i = 0; i < count; i++)
                    {
                        dataTypeNames[i] = dataReader.GetDataTypeName(i);
                    }
                }
            }

            var insertInto = new StringBuilder();
            insertInto.AppendFormat("insert into [{0}](", destinationTableName);
            var values = new StringBuilder();
            values.Append("values(");
            DataRowCollection schemaRows = schemaTable.Rows;
            count = schemaRows.Count;
            converters = new Converter<object, object>[count];
            insertCommand = destinationconnection.CreateCommand();

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    insertInto.Append(',');
                    values.Append(',');
                }

                var columnSchema = new DataColumnSchema(schemaRows[i]);
                insertInto.AppendFormat("[{0}]", columnSchema.ColumnName);
                values.AppendFormat("@p{0}", i);

                int columnSize = columnSchema.ColumnSize;
                int providerType = columnSchema.ProviderType;
                DbType dbType = (DbType)providerType;
                var parameter = new SqlParameter();
                parameter.ParameterName = string.Format("@p{0}", i);
                //parameter.DbType = dbType;
                insertCommand.Parameters.Add(parameter);

                switch (dataTypeNames[i].ToLower())
                {
                    case SqlDataTypeName.BigInt:
                        parameter.SqlDbType = SqlDbType.BigInt;
                        break;

                    case SqlDataTypeName.Bit:
                        parameter.SqlDbType = SqlDbType.Bit;
                        break;

                    case SqlDataTypeName.DateTime:
                        parameter.SqlDbType = SqlDbType.DateTime;
                        break;

                    case SqlDataTypeName.Float:
                        parameter.SqlDbType = SqlDbType.Float;
                        break;

                    case SqlDataTypeName.Int:
                        parameter.SqlDbType = SqlDbType.Int;
                        break;

                    case SqlDataTypeName.SmallDateTime:
                        parameter.SqlDbType = SqlDbType.SmallDateTime;
                        break;

                    case SqlDataTypeName.SmallInt:
                        parameter.SqlDbType = SqlDbType.SmallInt;
                        break;

                    case SqlDataTypeName.TinyInt:
                        parameter.SqlDbType = SqlDbType.TinyInt;
                        break;

                    case SqlDataTypeName.VarChar:
                    case SqlDataTypeName.NVarChar:
                    case SqlDataTypeName.Char:
                    case SqlDataTypeName.NChar:
                        parameter.Size = columnSchema.ColumnSize;
                        converters[i] = ConvertToString;
                        break;

                    case SqlDataTypeName.NText:
                        parameter.SqlDbType = SqlDbType.NText;
                        converters[i] = ConvertToString;
                        break;

                    case SqlDataTypeName.Decimal:
                        parameter.SqlDbType = SqlDbType.Decimal;
                        parameter.Precision = (byte)columnSchema.NumericPrecision.Value;
                        parameter.Scale = (byte)columnSchema.NumericScale.Value;
                        converters[i] = ConvertToDecimal;
                        break;

                    case SqlDataTypeName.Money:
                        parameter.SqlDbType = SqlDbType.Money;
                        converters[i] = ConvertToDecimal;
                        break;

                    case SqlDataTypeName.Xml:
                        parameter.SqlDbType = SqlDbType.Xml;
                        converters[i] = ConvertToString;
                        break;

                    default:
                        break;
                }
            }

            insertInto.Append(") ");
            values.Append(')');
            insertInto.Append(values);
            insertCommand.CommandText = insertInto.ToString();
        }

        void IProvider.DeriveParameters(IDbCommand command)
        {
            var sqlConnection = (SqlConnection)command.Connection;
            var sqlCommand = new SqlCommand(command.CommandText, sqlConnection)
            {
                CommandType = command.CommandType,
                CommandTimeout = command.CommandTimeout
            };

            try
            {
                SqlCommandBuilder.DeriveParameters(sqlCommand);
            }
            catch (Exception e)
            {
                string message = ((IProvider)this).GetExceptionMessage(e);
                log.Write(LogLevel.Error, message);
            }

            command.Parameters.Clear();

            while (sqlCommand.Parameters.Count > 0)
            {
                var parameter = sqlCommand.Parameters[0];
                sqlCommand.Parameters.RemoveAt(0);
                command.Parameters.Add(parameter);
            }
        }

        public XmlReader ExecuteXmlReader(IDbCommand command)
        {
            var sqlCommand = (SqlCommand)command;
            return sqlCommand.ExecuteXmlReader();
        }

        string IProvider.GetColumnTypeName(IProvider sourceProvider, DataRow sourceSchemaRow, string sourceDataTypeName)
        {
            DataColumnSchema schemaRow = new DataColumnSchema(sourceSchemaRow);
            int columnSize = schemaRow.ColumnSize;
            bool? allowDBNull = schemaRow.AllowDBNull;
            Type dataType = schemaRow.DataType;
            TypeCode typeCode = Type.GetTypeCode(dataType);
            string typeName;

            switch (typeCode)
            {
                case TypeCode.Int32:
                    typeName = SqlDataTypeName.Int;
                    break;

                case TypeCode.DateTime:
                    typeName = SqlDataTypeName.DateTime;
                    break;

                case TypeCode.Double:
                    typeName = SqlDataTypeName.Float;
                    break;

                case TypeCode.String:
                    typeName = string.Format("{0}({1})", SqlDataTypeName.NVarChar, columnSize);
                    break;

                default:
                    // TODO
                    typeName = string.Format("'{0}'", typeCode);
                    break;
            }

            return typeName;
        }

        Type IProvider.GetColumnType(DataColumnSchema dataColumnSchema)
        {
            SqlDbType dbType = (SqlDbType)dataColumnSchema.ProviderType;
            int columnSize = dataColumnSchema.ColumnSize;
            Type type;

            switch (dbType)
            {
                case SqlDbType.BigInt:
                    type = typeof (long);
                    break;

                case SqlDbType.Bit:
                    type = typeof (bool);
                    break;

                case SqlDbType.DateTime:
                    //type = typeof(DateTimeField); DataTableView does not work in edit mode
                    type = typeof (object);
                    break;

                case SqlDbType.Int:
                    type = typeof (int);
                    break;

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.NText:
                    if (columnSize <= 8000)
                    {
                        type = typeof (string);
                    }
                    else
                    {
                        //type = typeof(StringField);
                        type = typeof (object);
                    }

                    break;

                case SqlDbType.SmallInt:
                    type = typeof (short);
                    break;

                default:
                    type = typeof (object);
                    break;
            }

            return type;
        }

        GetCompletionResponse IProvider.GetCompletion(
            ConnectionBase connection,
            IDbTransaction transaction,
            string text,
            int position)
        {
            var response = new GetCompletionResponse
            {
                FromCache = false
            };
            List<IObjectName> array = null;
            var sqlStatement = new SqlStatement(text);
            Token[] tokens = sqlStatement.Tokens;
            Token previousToken, currentToken;
            sqlStatement.FindToken(position, out previousToken, out currentToken);

            if (currentToken != null)
            {
                var parts = new IdentifierParser(new StringReader(currentToken.Value)).Parse().ToList();
                var lastPart = parts.Last();
                int lastPartLength = lastPart != null ? lastPart.Length : 0;
                response.StartPosition = currentToken.EndPosition - lastPartLength + 1;
                response.Length = lastPartLength;
                string value = currentToken.Value;
                if (value.Length > 0 && value[0] == '@')
                {
                    if (value.IndexOf("@@") == 0)
                    {
                        array = keyWords.Where(k => k.StartsWith(value)).Select(keyWord => (IObjectName)new NonSqlObjectName(keyWord)).ToList();
                    }
                    else
                    {
                        var list = new SortedList<string, object>();

                        for (int i = 0; i < tokens.Length; i++)
                        {
                            var token = tokens[i];
                            string keyWord = token.Value;

                            if (keyWord != null && keyWord.Length >= 2 && keyWord.IndexOf(value) == 0 && keyWord != value)
                            {
                                if (!list.ContainsKey(token.Value))
                                {
                                    list.Add(token.Value, null);
                                }
                            }
                        }

                        array = list.Keys.Select(keyWord => (IObjectName)new NonSqlObjectName(keyWord)).ToList();
                    }
                }
            }
            else
            {
                response.StartPosition = position;
                response.Length = 0;
            }

            if (array == null)
            {
                var sqlObject = sqlStatement.FindSqlObject(previousToken, currentToken);
                string commandText = null;

                if (sqlObject != null)
                {
                    DatabaseObjectMultipartName name;
                    int i;

                    switch (sqlObject.Type)
                    {
                        case SqlObjectTypes.Database:
                            commandText = SqlServerObject.GetDatabases();
                            break;

                        case SqlObjectTypes.Table:
                        case SqlObjectTypes.View:
                        case SqlObjectTypes.Function:
                        case SqlObjectTypes.Table | SqlObjectTypes.View:
                        case SqlObjectTypes.Table | SqlObjectTypes.View | SqlObjectTypes.Function:
                        {
                            name = new DatabaseObjectMultipartName(connection.Database, sqlObject.Name);
                            List<string> nameParts = sqlObject.Name != null
                                ? new IdentifierParser(new StringReader(sqlObject.Name)).Parse().ToList()
                                : null;
                            int namePartsCount = nameParts != null ? nameParts.Count : 0;
                            var statements = new List<string>();

                            switch (namePartsCount)
                            {
                                case 0:
                                case 1:
                                {
                                    statements.Add(SqlServerObject.GetDatabases());
                                    statements.Add(SqlServerObject.GetSchemas());

                                    var objectTypes = sqlObject.Type.ToObjectTypes();
                                    statements.Add(SqlServerObject.GetObjects(schema: "dbo", objectTypes: objectTypes));
                                }
                                    break;

                                case 2:
                                    if (nameParts[0] != null)
                                    {
                                        statements.Add(SqlServerObject.GetSchemas(database: nameParts[0]));

                                        var objectTypes = sqlObject.Type.ToObjectTypes();
                                        statements.Add(SqlServerObject.GetObjects(schema: nameParts[0], objectTypes: objectTypes));
                                    }
                                    break;

                                case 3:
                                {
                                    if (nameParts[0] != null && nameParts[1] != null)
                                    {
                                        var objectTypes = sqlObject.Type.ToObjectTypes();
                                        statements.Add(SqlServerObject.GetObjects(database: nameParts[0], schema: nameParts[1], objectTypes: objectTypes));
                                    }
                                }
                                    break;
                            }

                            commandText = statements.Count > 0 ? string.Join("\r\n", statements) : null;
                        }
                            break;

                        case SqlObjectTypes.Column:
                            name = new DatabaseObjectMultipartName(connection.Database, sqlObject.ParentName);
                            string[] owners;

                            if (name.Schema != null)
                            {
                                owners = new string[] {name.Schema};
                            }
                            else
                            {
                                owners = new string[] {"dbo", "sys"};
                            }
                            var sb = new StringBuilder();
                            for (i = 0; i < owners.Length; i++)
                            {
                                if (i > 0)
                                {
                                    sb.Append(',');
                                }
                                sb.AppendFormat("'{0}'", owners[i]);
                            }
                            string ownersString = sb.ToString();
                            commandText = string.Format(@"declare @schema_id int
select  top 1 @schema_id = s.schema_id
from    [{0}].sys.schemas s
where   s.name  in({1})

if @schema_id is not null
begin
    declare @object_id int
    select  @object_id = o.object_id
    from    [{0}].sys.all_objects o
    where   o.name = '{2}'
            and o.schema_id = @schema_id
            and o.type in('S','U','TF','V')

    if @object_id is not null
    begin
        select  name
        from [{0}].sys.all_columns c
        where c.object_id = @object_id
        order by column_id
    end
end", name.Database, ownersString, name.Name);
                            break;

                        case SqlObjectTypes.Procedure:
                            name = new DatabaseObjectMultipartName(connection.Database, sqlObject.Name);

                            if (name.Schema == null)
                            {
                                name.Schema = "dbo";
                            }

                            commandText = string.Format(@"select
     s.name
    ,o.name
from [{0}].sys.objects o
join [{0}].sys.schemas s
on o.schema_id = s.schema_id
where   o.type in('P','X')
order by 1", name.Database);
                            break;

                        case SqlObjectTypes.Trigger:
                            commandText = "select name from sysobjects where xtype = 'TR' order by name";
                            break;

                        case SqlObjectTypes.Value:
                            string[] items = sqlObject.ParentName.Split('.');
                            i = items.Length - 1;
                            string columnName = items[i];
                            string tableNameOrAlias = null;
                            if (i > 0)
                            {
                                i--;
                                tableNameOrAlias = items[i];
                            }
                            if (tableNameOrAlias != null)
                            {
                                string tableName;
                                bool contains = sqlStatement.Tables.TryGetValue(tableNameOrAlias, out tableName);
                                if (contains)
                                {
                                    string where;
                                    int tokenIndex = previousToken.Index + 1;
                                    if (tokenIndex < tokens.Length)
                                    {
                                        var token = tokens[tokenIndex];
                                        string tokenValue = token.Value;
                                        int indexofAny = tokenValue.IndexOfAny(new char[] {'\r', '\n'});
                                        if (indexofAny >= 0)
                                        {
                                            tokenValue = tokenValue.Substring(0, indexofAny);
                                        }
                                        string like;
                                        if (tokenValue.Length > 0)
                                        {
                                            if (tokenValue.Contains('%'))
                                            {
                                                like = tokenValue;
                                            }
                                            else
                                            {
                                                like = tokenValue + '%';
                                            }
                                        }
                                        else
                                        {
                                            like = "%";
                                        }
                                        @where = string.Format("where {0} like N'{1}'", columnName, like);
                                    }
                                    else
                                    {
                                        @where = null;
                                    }
                                    commandText = string.Format("select distinct top 100 {0} from {1} (readpast) {2} order by 1", columnName, tableName, @where);
                                }
                            }
                            break;
                    }
                }

                if (commandText != null)
                {
                    log.Write(LogLevel.Trace, "commandText:\r\n{0}", commandText);
                    var list = new List<IObjectName>();
                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }

                        using (var context = connection.Connection.ExecuteReader(transaction, commandText, CommandType.Text, 0, CommandBehavior.Default))
                        {
                            var dataReader = context.DataReader;

                            while (true)
                            {
                                int fieldCount = dataReader.FieldCount;
                                while (dataReader.Read())
                                {
                                    string schemaName;
                                    string objectName;

                                    if (fieldCount == 1)
                                    {
                                        schemaName = null;
                                        objectName = dataReader[0].ToString();
                                    }
                                    else
                                    {
                                        schemaName = dataReader.GetValueOrDefault<string>(0);
                                        objectName = dataReader.GetString(1);
                                    }

                                    list.Add(new ObjectName(sqlObject, schemaName, objectName));
                                }

                                if (!dataReader.NextResult())
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }

                    array = list;
                }
            }

            response.Items = array;
            return response;
        }

        DataParameterBase IProvider.GetDataParameter(IDataParameter parameter)
        {
            var sqlParameter = (SqlParameter)parameter;
            return new SqlDataParameter(sqlParameter);
        }

        string IProvider.GetExceptionMessage(Exception exception)
        {
            string message;
            var sqlException = exception as SqlException;

            if (sqlException != null)
            {
                message = sqlException.Errors.ToLogString();
            }
            else
            {
                message = exception.ToString();
            }

            return message;
        }

        DataTable IProvider.GetParameterTable(IDataParameterCollection parameters)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(" ");
            dataTable.Columns.Add("ParameterName");
            dataTable.Columns.Add("DbType");
            dataTable.Columns.Add("SqlDbType");
            dataTable.Columns.Add("Size", typeof (int));
            dataTable.Columns.Add("Precision", typeof (int));
            dataTable.Columns.Add("Scale", typeof (int));
            dataTable.Columns.Add("Direction");
            dataTable.Columns.Add("Value", typeof (object));
            int index = 0;

            foreach (SqlParameter p in parameters)
            {
                DataRow row = dataTable.NewRow();

                row[0] = index;
                row[1] = p.ParameterName;
                row[2] = p.DbType.ToString("G");
                row[3] = p.SqlDbType.ToString().ToLower();

                byte precision = p.Precision;
                int size;

                if (precision > 0)
                {
                    if (precision <= 9)
                        size = 5;
                    else if (precision <= 19)
                        size = 9;
                    else if (precision <= 28)
                        size = 13;
                    else
                        size = 17;
                }
                else
                {
                    size = p.Size;
                }

                row[4] = size;
                row[5] = p.Precision;
                row[6] = p.Scale;
                row[7] = p.Direction.ToString("G");

                if (p.Value == null)
                    row[8] = DBNull.Value;
                else
                    row[8] = p.Value;

                dataTable.Rows.Add(row);

                index++;
            }

            return dataTable;
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
                    var dbType = (SqlDbType)dataColumnSchema.ProviderType;
                    string dataTypeName = dataReader.GetDataTypeName(columnIndex);
                    var sb = new StringBuilder();
                    sb.Append(dataTypeName);

                    switch (dbType)
                    {
                        case SqlDbType.Char:
                        case SqlDbType.VarChar:
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                        case SqlDbType.Binary:
                        case SqlDbType.VarBinary:
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

                        case SqlDbType.Decimal:
                            short precision = dataColumnSchema.NumericPrecision.GetValueOrDefault();
                            short scale = dataColumnSchema.NumericScale.GetValueOrDefault();

                            if (scale == 0)
                                sb.AppendFormat("({0})", precision);
                            else
                                sb.AppendFormat("({0},{1})", precision, scale);

                            if (precision <= 9)
                                columnSize = 5;
                            else if (precision <= 19)
                                columnSize = 9;
                            else if (precision <= 28)
                                columnSize = 13;
                            else
                                columnSize = 17;
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
            var sqlStatement = new SqlStatement(commandText);
            var tokens = sqlStatement.Tokens;
            var statements = new List<string>();

            foreach (var statementTokens in tokens.Split(token => IsBatchSeparator(commandText, token)).Where(statementTokens => statementTokens.Length > 0))
            {
                int startIndex = statementTokens[0].StartPosition;
                int endIndex = statementTokens.Last().EndPosition;
                int length = endIndex - startIndex + 1;
                string statement = commandText.Substring(startIndex, length);
                statements.Add(statement);
            }

            return statements;
        }

        DataSet IProvider.GetTableSchema(IDbConnection connection, string tableName)
        {
            var sqlCommandBuilder = new SqlCommandBuilder();

            var fourPartName = new DatabaseObjectMultipartName(connection.Database, tableName);
            string owner = fourPartName.Schema;
            if (owner == null)
            {
                owner = "dbo";
            }

            string commandText = string.Format(@"declare @id int

select  @id = o.object_id
from    {0}.sys.objects o
join    {0}.sys.schemas s
on      o.schema_id = s.schema_id
where   s.name = {1}
        and o.name = {2}

select
    c.name                                                                              as ColumnName,
    c.colid                                                                             as ColumnOrdinal,
    convert(bit,case when c.cdefault <> 0 then 1 else 0 end)                            as HasDefault,
    convert(bit,c.isnullable)                                                           as IsNullable,
    convert(bit,case when c.autoval is not null or c.status = 128 then 1 else 0 end)    as HasAutomaticValue 
from	{0}.dbo.syscolumns c
where	c.id		= @id
order by c.colid

/*select	k.colid		as ColumnOrdinal
from	{0}.dbo.sysobjects o
join	{0}.dbo.sysindexes i
on	o.name		= i.name
join	{0}.dbo.sysindexkeys k
on	i.id		= k.id
	and i.indid	= k.indid
where	o.parent_obj	= @id
	and xtype	= 'PK'
order by k.keyno*/

declare @index_id int

select  @index_id = i.index_id
from    {0}.sys.indexes i (readpast)
where
    i.object_id = @id
    and i.is_primary_key = 1

if @index_id is null
begin
    select  @index_id = i.index_id
    from    {0}.sys.indexes i (readpast)
    where
        i.object_id = @id
        and i.is_unique = 1
end

select  ic.column_id as ColumnOrdinal
from    {0}.sys.index_columns ic
where
    ic.object_id    = @id
    and ic.index_id = @index_id
order by ic.index_column_id
",
                sqlCommandBuilder.QuoteIdentifier(fourPartName.Database),
                owner.ToTSqlNVarChar(),
                fourPartName.Name.ToTSqlNVarChar());
            log.Write(LogLevel.Trace, commandText);
            DataSet dataSet = connection.ExecuteDataSet(commandText);
            return dataSet;
        }

        List<InfoMessage> IProvider.ToInfoMessages(Exception exception)
        {
            DateTime now = LocalTime.Default.Now;
            List<InfoMessage> infoMessages;
            var sqlException = exception as SqlException;
            if (sqlException != null)
            {
                infoMessages = ToInfoMessages(sqlException.Errors);
            }
            else
            {
                string message = exception.ToLogString();
                var infoMessage = new InfoMessage(now, InfoMessageSeverity.Error, message);
                infoMessages = new List<InfoMessage>
                {
                    infoMessage
                };
            }

            return infoMessages;
        }

        #endregion

        #endregion

        internal static List<InfoMessage> ToInfoMessages(SqlErrorCollection sqlErrors)
        {
            Contract.Requires<ArgumentNullException>(sqlErrors != null);
            DateTime now = LocalTime.Default.Now;
            int count = sqlErrors.Count;
            var messages = new List<InfoMessage>(sqlErrors.Count);

            foreach (SqlError sqlError in sqlErrors)
            {
                var severity = sqlError.Class == 0 ? InfoMessageSeverity.Information : InfoMessageSeverity.Error;
                string message = sqlError.ToLogString();
                messages.Add(new InfoMessage(now, severity, message));
            }

            return messages;
        }

        #region Private Methods

        private static object ConvertToString(object source)
        {
            object target;
            if (source == null || source == DBNull.Value)
            {
                target = DBNull.Value;
            }
            else
            {
                var convertible = (IConvertible)source;
                target = convertible.ToString(null);
            }
            return target;
        }

        private static object ConvertToDecimal(object source)
        {
            object target;
            if (source == DBNull.Value)
            {
                target = DBNull.Value;
            }
            else
            {
                var decimalField = (DecimalField)source;
                target = decimalField.DecimalValue;
            }
            return target;
        }

        private static bool IsBatchSeparator(string commandText, Token token)
        {
            bool isBatchSeparator =
                token.Type == TokenType.KeyWord &&
                string.Compare(token.Value, "GO", StringComparison.InvariantCultureIgnoreCase) == 0;

            if (isBatchSeparator)
            {
                int lineStartIndex = commandText.LastIndexOf('\n', token.StartPosition);
                lineStartIndex++;
                int lineEndIndex = commandText.IndexOf('\n', token.EndPosition + 1);
                if (lineEndIndex == -1)
                {
                    lineEndIndex = commandText.Length - 1;
                }
                int lineLength = lineEndIndex - lineStartIndex + 1;
                string line = commandText.Substring(lineStartIndex, lineLength);
                line = line.Trim();
                isBatchSeparator = string.Compare(line, "GO", StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            return isBatchSeparator;
        }

        #endregion
    }
}