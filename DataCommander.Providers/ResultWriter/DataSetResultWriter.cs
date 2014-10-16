namespace DataCommander.Providers
{
    using System;
    using System.Data;
    using DataCommander.Foundation.Data;
    using DataCommander.Foundation.Diagnostics;

    /// <summary>
    /// Summary description for DataSetResultWriter.
    /// </summary>
    internal sealed class DataSetResultWriter : IResultWriter
    {
        #region Private Fields

        private IResultWriter logResultWriter;
        private QueryForm queryForm;
        private bool showShemaTable;
        private IProvider provider;
        private DataSet dataSet;
        private DataTable dataTable;
        private int rowIndex;

        private delegate void WriteTableBeginDelegate(int tableIndex, DataTable schemaTable, string[] dataTypeNames);

        #endregion

        public DataSetResultWriter(
            Action<InfoMessage> addInfoMessage,
            QueryForm queryForm,
            bool showShemaTable)
        {
            this.logResultWriter = new LogResultWriter(addInfoMessage);
            this.queryForm = queryForm;
            this.showShemaTable = showShemaTable;
        }

        #region Public Properties

        public DataSet DataSet
        {
            get
            {
                return dataSet;
            }
        }

        #endregion

        #region IResultWriter Members

        void IResultWriter.Begin()
        {
            this.logResultWriter.Begin();
        }

        void IResultWriter.BeforeExecuteReader(IProvider provider, IDbCommand command)
        {
            this.logResultWriter.BeforeExecuteReader(provider, command);
            this.provider = provider;
        }

        void IResultWriter.AfterExecuteReader()
        {
            this.logResultWriter.AfterExecuteReader();
            this.dataSet = new DataSet();
        }

        void IResultWriter.AfterCloseReader(int affectedRows)
        {
            this.logResultWriter.AfterCloseReader(affectedRows);
        }

        void IResultWriter.WriteTableBegin(DataTable schemaTable, string[] dataTypeNames)
        {
            this.logResultWriter.WriteTableBegin(schemaTable, dataTypeNames);
            this.CreateTable(schemaTable, dataTypeNames);
        }

        void IResultWriter.FirstRowReadBegin()
        {
            this.logResultWriter.FirstRowReadBegin();
        }

        void IResultWriter.FirstRowReadEnd()
        {
            this.logResultWriter.FirstRowReadEnd();
        }

        void IResultWriter.WriteRows(object[][] rows, int rowCount)
        {
            MethodProfiler.BeginMethod();
            this.logResultWriter.WriteRows(rows, rowCount);

            try
            {
                var targetRows = dataTable.Rows;

                for (int i = 0; i < rowCount; i++)
                {
                    targetRows.Add(rows[i]);
                }

                rowIndex += rowCount;
            }
            finally
            {
                MethodProfiler.EndMethod();
            }
        }

        void IResultWriter.WriteTableEnd()
        {
            this.logResultWriter.WriteTableEnd();
        }

        void IResultWriter.WriteParameters(IDataParameterCollection parameters)
        {
            // TODO TextResultWriter.WriteParameters(parameters, textWriter, queryForm);
        }

        void IResultWriter.End()
        {
            this.logResultWriter.End();

            //int last = this.tableCount - 1;
            //if (last >= 0)
            //{
            //    DataTable table = dataSet.Tables[ last ];
            //    string name = string.Format( "DataTable({0})", table.Rows.Count );
            //    GarbageMonitor.Add( name, dataTable );
            //}
        }

        #endregion

        #region Private Methods

        private void CreateTable(DataTable schemaTable, string[] dataTypeNames)
        {
            int tableCount = this.dataSet.Tables.Count + 1;
            string tableName = schemaTable.TableName;
            if (tableName == "SchemaTable")
            {
                tableName = string.Format("Table {0}", tableCount);
            }
            if (showShemaTable)
            {
                schemaTable.TableName = string.Format("Schema {0}", tableCount);
                this.dataSet.Tables.Add(schemaTable);
            }
            this.dataTable = dataSet.Tables.Add();
            if (!string.IsNullOrEmpty(tableName))
            {
                this.dataTable.TableName = tableName;
            }
            foreach (DataRow schemaRow in schemaTable.Rows)
            {
                var dataColumnSchema = new DataColumnSchema(schemaRow);
                string columnName = dataColumnSchema.ColumnName;
                int columnSize = dataColumnSchema.ColumnSize;
                Type dataType = this.provider.GetColumnType(dataColumnSchema);

                DataColumn dataColumn;
                int n = 2;
                string columnName2 = columnName;

                while (true)
                {
                    if (dataTable.Columns.Contains(columnName2))
                    {
                        columnName2 = columnName + n;
                        n++;
                    }
                    else
                    {
                        columnName = columnName2;

                        if (dataType != null)
                        {
                            dataColumn = dataTable.Columns.Add(columnName, dataType);
                        }
                        else
                        {
                            dataColumn = dataTable.Columns.Add(columnName);
                        }

                        //dataColumn.AllowDBNull = sr.AllowDBNull == true;                                
                        //dataColumn.Unique = sr.IsUnique == true; // TFS provider does not support this column
                        dataColumn.ExtendedProperties.Add(0, schemaRow["DataType"]);
                        break;
                    }
                }
            }
        }

        #endregion
    }
}