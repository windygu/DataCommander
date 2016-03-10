namespace DataCommander.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using DataCommander.Foundation.Data;
    using DataCommander.Foundation.Diagnostics;
    using DataCommander.Foundation.Threading;
    using ThreadState = System.Threading.ThreadState;

    internal sealed class AsyncDataAdapter : IAsyncDataAdapter
    {
        #region Private Fields

        private static readonly ILog log = LogFactory.Instance.GetCurrentTypeLog();
        private IProvider provider;
        private IEnumerable<AsyncDataAdapterCommand> commands;
        private AsyncDataAdapterCommand command;
        private int maxRecords;
        private int rowBlockSize;
        private IResultWriter resultWriter;
        private Action<IAsyncDataAdapter, Exception> endFill;
        private Action<IAsyncDataAdapter> writeEnd;
        private long rowCount;
        private WorkerThread thread;
        private int tableCount;
        private bool isCommandCanceled;

        #endregion

        #region IAsyncDataAdapter Members

        IResultWriter IAsyncDataAdapter.ResultWriter => this.resultWriter;

        long IAsyncDataAdapter.RowCount => this.rowCount;

        int IAsyncDataAdapter.TableCount => this.tableCount;

        void IAsyncDataAdapter.BeginFill(
            IProvider provider,
            IEnumerable<AsyncDataAdapterCommand> commands,
            int maxRecords,
            int rowBlockSize,
            IResultWriter resultWriter,
            Action<IAsyncDataAdapter, Exception> endFill,
            Action<IAsyncDataAdapter> writeEnd)
        {
            this.provider = provider;
            this.commands = commands;
            this.maxRecords = maxRecords;
            this.rowBlockSize = rowBlockSize;
            this.resultWriter = resultWriter;
            this.endFill = endFill;
            this.writeEnd = writeEnd;

            if (commands != null)
            {
                this.thread = new WorkerThread(this.Fill)
                {
                    Name = "AsyncDataAdapter.Fill"
                };

                this.thread.Start();
            }
            else
            {
                writeEnd(this);
            }
        }

        void IAsyncDataAdapter.Cancel()
        {
            using (LogFactory.Instance.GetCurrentMethodLog())
            {
                this.isCommandCanceled = true;
                if (this.thread != null)
                {
                    this.thread.Stop();
                    if (this.provider.IsCommandCancelable)
                    {
                        ThreadPool.QueueUserWorkItem(this.CancelWaitCallback);
                    }
                    else
                    {
                        bool joined = this.thread.Join(5000);

                        if (!joined)
                        {
                            this.thread.Abort();
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void ReadTable(
            IDataReader dataReader,
            DataTable schemaTable,
            int tableIndex)
        {
            Contract.Requires<ArgumentNullException>(dataReader != null);
            Contract.Requires<ArgumentNullException>(schemaTable != null);
            Contract.Requires<ArgumentOutOfRangeException>(tableIndex >= 0);

            using (LogFactory.Instance.GetCurrentMethodLog())
            {
                Exception exception = null;
                IDataReaderHelper dataReaderHelper = this.provider.CreateDataReaderHelper(dataReader);
                DataRowCollection schemaRows = schemaTable.Rows;
                int count = schemaRows.Count;

                this.resultWriter.WriteTableBegin(schemaTable);

                int fieldCount = dataReader.FieldCount;

                if (fieldCount < 0)
                {
                    fieldCount = 0;
                }

                var rows = new object[this.rowBlockSize][];
                int i;

                for (i = 0; i < this.rowBlockSize; i++)
                {
                    rows[i] = new object[fieldCount];
                }

                this.rowCount = 0;
                i = 0;
                bool first = true;
                bool exitFromWhile = false;
                var stopwatch = Stopwatch.StartNew();

                while (!this.isCommandCanceled && !this.thread.IsStopRequested && !exitFromWhile)
                {
                    bool read;

                    if (first)
                    {
                        first = false;
                        this.resultWriter.FirstRowReadBegin();
                        read = dataReader.Read();

                        string[] dataTypeNames = new string[count];

                        if (read)
                        {
                            for (int j = 0; j < count; j++)
                            {
                                dataTypeNames[j] = dataReader.GetDataTypeName(j);
                            }
                        }

                        this.resultWriter.FirstRowReadEnd(dataTypeNames);
                    }
                    else
                    {
                        try
                        {
                            read = dataReader.Read();
                        }
                        catch (Exception e)
                        {
                            read = false;
                            exception = e;
                        }
                    }

                    if (read)
                    {
                        this.rowCount++;
                        dataReaderHelper.GetValues(rows[i]);
                        i++;

                        if (i == this.rowBlockSize || stopwatch.ElapsedMilliseconds >= 5000)
                        {
                            this.resultWriter.WriteRows(rows, i);
                            i = 0;
                            stopwatch.Restart();
                        }

                        if (this.rowCount == this.maxRecords)
                        {
                            this.CancelWaitCallback(null);
                            break;
                        }
                    }
                    else
                    {
                        exitFromWhile = true;
                    }
                }

                if (i != this.rowBlockSize)
                {
                    log.Write(LogLevel.Trace, "resultWriter.WriteRows(rows,i);");
                    this.resultWriter.WriteRows(rows, i);
                }

                log.Write(LogLevel.Trace, "resultWriter.WriteTableEnd(rowCount);");
                this.resultWriter.WriteTableEnd();

                if (this.rowCount > 0)
                {
                    this.tableCount++;
                }

                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        private void Fill(AsyncDataAdapterCommand asyncDataAdapterCommand)
        {
            Contract.Requires<ArgumentNullException>(asyncDataAdapterCommand != null);
            var command = asyncDataAdapterCommand.Command;

            Exception exception = null;

            try
            {
                this.resultWriter.BeforeExecuteReader(asyncDataAdapterCommand);
                IDataReader dataReader = null;
                try
                {
                    dataReader = command.ExecuteReader();
                    int fieldCount = dataReader.FieldCount;
                    this.resultWriter.AfterExecuteReader(fieldCount);
                    int tableIndex = 0;

                    while (!this.thread.IsStopRequested)
                    {
                        if (fieldCount > 0)
                        {
                            DataTable schemaTable = dataReader.GetSchemaTable();
                            if (schemaTable != null)
                            {
                                log.Trace("schemaTable:\r\n{0}", schemaTable.ToStringTable());
                            }

                            this.ReadTable(dataReader, schemaTable, tableIndex);
                        }

                        if (this.rowCount >= this.maxRecords || !dataReader.NextResult())
                        {
                            break;
                        }

                        tableIndex++;
                    }
                }
                finally
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                        int recordsAffected = dataReader.RecordsAffected;
                        this.resultWriter.AfterCloseReader(recordsAffected);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                if (Thread.CurrentThread.ThreadState == ThreadState.AbortRequested)
                {
                    try
                    {
                        Thread.ResetAbort();
                    }
                    catch
                    {
                    }
                }

                exception = e;
            }
            finally
            {
                if (command != null && command.Parameters != null)
                {
                    this.resultWriter.WriteParameters(command.Parameters);
                }
                long ticks = Stopwatch.GetTimestamp();
                this.endFill(this, exception);
                ticks = Stopwatch.GetTimestamp() - ticks;
                log.Write(LogLevel.Trace, "this.endFill( this, exception ); completed in {0} seconds.", StopwatchTimeSpan.ToString(ticks, 3));
            }
        }

        private void Fill()
        {
            this.resultWriter.Begin(this.provider);

            try
            {
                foreach (var command in this.commands)
                {
                    this.command = command;
                    this.Fill(command);
                    command.Command.Dispose();
                }
            }
            finally
            {
                this.resultWriter.End();
                this.writeEnd(this);
            }
        }

        private void CancelWaitCallback(object state)
        {
            using (LogFactory.Instance.GetCurrentMethodLog())
            {
                this.command.Command.Cancel();
            }
        }

        #endregion
    }
}