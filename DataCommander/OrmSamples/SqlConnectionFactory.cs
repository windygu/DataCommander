using System.Data.SqlClient;

namespace OrmSamples
{
    public static class SqlConnectionFactory
    {
        public static SqlConnection Create()
        {
            var sqlConnectionString = GetSqlConnectionString();
            return new SqlConnection(sqlConnectionString);
        }

        private static string GetSqlConnectionString()
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            sqlConnectionStringBuilder.DataSource = @".\SQL2017_001";
            sqlConnectionStringBuilder.InitialCatalog = "Sandbox";
            sqlConnectionStringBuilder.IntegratedSecurity = true;
            return sqlConnectionStringBuilder.ConnectionString;
        }
    }
}