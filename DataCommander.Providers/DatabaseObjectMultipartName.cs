namespace DataCommander.Providers
{
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// See <see cref="http://msdn.microsoft.com/en-us/library/ms177563.aspx"/>.
    /// </summary>
    public sealed class DatabaseObjectMultipartName
    {
        private string server;
        private string database;
        private string schema;
        private readonly string name;

        public DatabaseObjectMultipartName(string server, string database, string schema, string name)
        {
            this.server = server;
            this.database = database;
            this.schema = schema;
            this.name = name;
        }

        public DatabaseObjectMultipartName(string currentDatabase, string name)
        {
            if (name != null)
            {
                var parser = new IdentifierParser(new StringReader(name));
                var parts = parser.Parse().ToArray();

                int i = parts.Length - 1;
                var commandBuilder = new SqlCommandBuilder();

                if (i >= 0)
                {
                    this.name = parts[i];
                    i--;

                    if (i >= 0)
                    {
                        this.schema = parts[i];
                        i--;

                        if (i >= 0)
                        {
                            this.database = parts[i];
                            i--;

                            if (i >= 0)
                            {
                                this.server = parts[i];
                            }
                        }
                    }
                }
            }

            if (database == null)
            {
                database = currentDatabase;
            }

            if (string.IsNullOrEmpty(schema))
            {
                schema = null;
            }

            if (this.name != null)
            {
                int length = this.name.Length;

                if (length > 0 && this.name[0] == '[')
                {
                    this.name = this.name.Substring(1);
                    length--;
                }

                if (length > 0 && this.name[length - 1] == ']')
                {
                    this.name = this.name.Substring(0, length - 1);
                }
            }
        }

        public string Database
        {
            get
            {
                return this.database;
            }

            set
            {
                this.database = value;
            }
        }

        public string Schema
        {
            get
            {
                return schema;
            }

            set
            {
                schema = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (this.database != null)
            {
                sb.Append(this.database);
            }

            if (this.schema != null)
            {
                if (sb.Length > 0)
                {
                    sb.Append('.');
                }
                sb.Append(this.schema);
            }

            if (sb.Length > 0)
            {
                sb.Append('.');
            }

            sb.Append(this.name);
            return sb.ToString();
        }
    }
}