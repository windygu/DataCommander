﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using DataCommander.Providers.Connection;
using Foundation.Data;
using OfficeOpenXml;

namespace DataCommander.Providers.ResultWriter
{
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
#if CONTRACTS_FULL
            Contract.Requires<ArgumentNullException>(provider != null);
            Contract.Requires<ArgumentNullException>(addInfoMessage != null);
#endif

            this.provider = provider;
            this.addInfoMessage = addInfoMessage;
            this.logResultWriter = new LogResultWriter(addInfoMessage);
        }

        #region IResultWriter Members

        void IResultWriter.Begin(IProvider provider)
        {
            this.logResultWriter.Begin(provider);
            this.provider = provider;
        }

        void IResultWriter.BeforeExecuteReader(AsyncDataAdapterCommand command)
        {
            this.logResultWriter.BeforeExecuteReader(command);
        }

        void IResultWriter.AfterExecuteReader(int fieldCount)
        {
            this.logResultWriter.AfterExecuteReader(fieldCount);

            var fileName = Path.GetTempFileName() + ".xlsx";
            this.excelPackage = new ExcelPackage(new FileInfo(fileName));
        }

        void IResultWriter.AfterCloseReader(int affectedRows)
        {
            this.logResultWriter.AfterCloseReader(affectedRows);
        }

        void IResultWriter.WriteTableBegin(DataTable schemaTable)
        {
            this.logResultWriter.WriteTableBegin(schemaTable);
            this.CreateTable(schemaTable);
        }

        void IResultWriter.FirstRowReadBegin()
        {
            this.logResultWriter.FirstRowReadBegin();
        }

        void IResultWriter.FirstRowReadEnd(string[] dataTypeNames)
        {
            this.logResultWriter.FirstRowReadEnd(dataTypeNames);
        }

        void IResultWriter.WriteRows(object[][] rows, int rowCount)
        {
            this.logResultWriter.WriteRows(rows, rowCount);

            var cells = this.excelWorksheet.Cells;

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var row = rows[rowIndex];

                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
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

        private void CreateTable(DataTable schemaTable)
        {
            var worksheets = this.excelPackage.Workbook.Worksheets;
            var tableName = $"Table{worksheets.Count + 1}";
            this.excelWorksheet = worksheets.Add(tableName);
            var cells = this.excelWorksheet.Cells;
            var columnIndex = 1;

            foreach (DataRow schemaRow in schemaTable.Rows)
            {
                var dataColumnSchema = new DbColumn(schemaRow);
                var columnName = dataColumnSchema.ColumnName;
                var columnSize = dataColumnSchema.ColumnSize;
                var dataType = this.provider.GetColumnType(dataColumnSchema);

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