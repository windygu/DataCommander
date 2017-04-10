namespace DataCommander.Foundation.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading;
    using DataCommander.Foundation.Linq;
    using DataCommander.Foundation.Threading;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class SqlLogWriter : ILogWriter
    {
        #region Private Fields

        private static readonly ILog log = InternalLogFactory.Instance.GetTypeLog(typeof (SqlLogWriter));
        private const int Period = 10000;
        private readonly Func<IDbConnection> createConnection;
        private readonly Func<LogEntry, string> logEntryToCommandText;
        private readonly int commandTimeout;
        private readonly SingleThreadPool singleThreadPool;
        private readonly List<LogEntry> entryQueue = new List<LogEntry>();
        private readonly object lockObject = new object();
        private Timer timer;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createConnection"></param>
        /// <param name="logEntryToCommandText"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="singleThreadPool"></param>
        public SqlLogWriter(
            Func<IDbConnection> createConnection,
            Func<LogEntry, string> logEntryToCommandText,
            int commandTimeout,
            SingleThreadPool singleThreadPool)
        {
#if CONTRACTS_FULL
            Contract.Requires<ArgumentNullException>(createConnection != null);
            Contract.Requires<ArgumentNullException>(logEntryToCommandText != null);
            Contract.Requires<ArgumentNullException>(singleThreadPool != null);
#endif

            this.createConnection = createConnection;
            this.logEntryToCommandText = logEntryToCommandText;
            this.singleThreadPool = singleThreadPool;
            this.commandTimeout = commandTimeout;
        }

#region ILogWriter Members

        void ILogWriter.Open()
        {
            this.timer = new Timer(this.TimerCallback, null, 0, Period);
        }

        void ILogWriter.Write(LogEntry logEntry)
        {
            lock (this.entryQueue)
            {
                this.entryQueue.Add(logEntry);
            }
        }

        private void Flush()
        {
            this.TimerCallback(null);
        }

        void ILogWriter.Flush()
        {
            this.Flush();
        }

        void ILogWriter.Close()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }

            this.Flush();
        }

#endregion

#region IDisposable Members

        void IDisposable.Dispose()
        {
            // TODO
        }

#endregion

        private void TimerCallback(object state)
        {
            lock (this.lockObject)
            {
                if (this.entryQueue.Count > 0)
                {
                    if (this.timer != null)
                    {
                        this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }

                    LogEntry[] array;

                    lock (this.entryQueue)
                    {
                        var count = this.entryQueue.Count;
                        array = new LogEntry[count];
                        this.entryQueue.CopyTo(array);
                        this.entryQueue.Clear();
                    }

                    this.singleThreadPool.QueueUserWorkItem(this.WaitCallback, array);

                    if (this.timer != null)
                    {
                        this.timer.Change(Period, Period);
                    }
                }
            }
        }

        private void WaitCallback(object state)
        {
            try
            {
                var array = (LogEntry[])state;
                var sb = new StringBuilder();
                string commandText;

                for (var i = 0; i < array.Length; i++)
                {
                    commandText = this.logEntryToCommandText(array[i]);
                    sb.AppendLine(commandText);
                }

                commandText = sb.ToString();

                using (var connection = this.createConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = commandText;
                    command.CommandTimeout = this.commandTimeout;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToLogString());
            }
        }
    }
}