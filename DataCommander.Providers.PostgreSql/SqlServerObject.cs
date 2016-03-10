namespace DataCommander.Providers.PostgreSql
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Foundation.Data.SqlClient;

    internal static class SqlServerObject
    {
        public static string GetSchemas()
        {
            return @"select schema_name
from information_schema.schemata
order by schema_name";
        }

        public static string GetTables(string schema, IEnumerable<string> tableTypes)
        {
            return
                $@"select table_name
from information_schema.tables
where
    table_schema = '{schema}'
    and table_type in({string.Join(",",
                    tableTypes.Select(o => o.ToTSqlVarChar()))})
order by table_name";
        }

        public static string GetObjects(string schema, IEnumerable<string> objectTypes)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(schema));
            Contract.Requires(objectTypes != null && objectTypes.Any());

            return
                $@"declare @schema_id int

select @schema_id = schema_id
from sys.schemas (nolock)
where name = '{schema
                    }'

if @schema_id is not null
begin
    select o.name
    from sys.all_objects o (nolock)
    where
        o.schema_id = @schema_id
        and o.type in({
                    string.Join(",", objectTypes.Select(o => o.ToTSqlVarChar()))})
    order by o.name
end";
        }

        public static string GetObjects(
            string database,
            string schema,
            IEnumerable<string> objectTypes)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(database));
            Contract.Requires(!string.IsNullOrWhiteSpace(schema));
            Contract.Requires(objectTypes!=null && objectTypes.Any());

            return string.Format(@"if exists(select * from sys.databases (nolock) where name = '{0}')
begin
    declare @schema_id int

    select @schema_id = schema_id
    from [{0}].sys.schemas (nolock)
    where name = '{1}'

    if @schema_id is not null
    begin
        select o.name
        from [{0}].sys.all_objects o (nolock)
        where
            o.schema_id = @schema_id
            and o.type in({2})
        order by o.name
    end
end", database, schema, string.Join(",", objectTypes.Select(t => t.ToTSqlVarChar())));
        }
    }
}