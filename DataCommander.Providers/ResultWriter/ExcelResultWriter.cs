﻿namespace DataCommander.Providers.ResultWriter
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using DataCommander.Foundation.Data;
    using OfficeOpenXml;

    internal sealed class ExcelResultWriter : IResultWriter
    {
        private IProvider provider;
        private readonly Action<InfoMessage> addInfoMessage;
        private readonly IResultWriter logResultWriter;
        private ExcelPackage excelPackage;
        private ExcelWorksheet excelWorksheet;
        private int rowCount;

        public ExcelResultWriter(
            IProvider provider,
            Action<InfoMessage> addInfoMessage)
        {
            Contract.Requires<ArgumentNullException>(provider != null);
            Contract.Requires<ArgumentNullException>(addInfoMessage != null);

            this.provider = provider;
            this.addInfoMessage = addInfoMessage;
            this.logResultWriter = new LogResultWriter(addInfoMessage);
        }

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

            string fileName = Path.GetTempFileName() + ".xlsx";
            this.excelPackage = new ExcelPackage(new FileInfo(fileName));
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
            this.logResultWriter.WriteRows(rows, rowCount);

            var cells = this.excelWorksheet.Cells;

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var row = rows[rowIndex];

                for (int columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    cells[this.rowCount + rowIndex, columnIndex + 1].Value = row[columnIndex];
                }
            }

            this.rowCount += rowCount;
        }

        void IResultWriter.WriteTableEnd()
        {
            this.logResultWriter.WriteTableEnd();
        }

        void IResultWriter.WriteParameters(IDataParameterCollection parameters)
        {
        }

        void IResultWriter.End()
        {
            this.logResultWriter.End();

            this.excelPackage.Save();

            Process.Start(this.excelPackage.File.FullName);
        }

        #endregion

        private void CreateTable(DataTable schemaTable, string[] dataTypeNames)
        {
            var worksheets = this.excelPackage.Workbook.Worksheets;
            string tableName = string.Format("Table{0}", worksheets.Count + 1);
            this.excelWorksheet = worksheets.Add(tableName);
            var cells = this.excelWorksheet.Cells;
            int columnIndex = 1;

            foreach (DataRow schemaRow in schemaTable.Rows)
            {
                var dataColumnSchema = new DataColumnSchema(schemaRow);
                string columnName = dataColumnSchema.ColumnName;
                int columnSize = dataColumnSchema.ColumnSize;
                Type dataType = this.provider.GetColumnType(dataColumnSchema);

                var cell = cells[1, columnIndex];
                cell.Value = columnName;
                cell.Style.Font.Bold = true;

                columnIndex++;
            }

            this.excelWorksheet.View.FreezePanes(2, 1);
            this.rowCount = 2;
        }
    }
}