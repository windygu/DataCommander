namespace DataCommander.Foundation.Diagnostics
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class LogEntry
    {
        #region Private Fields
        private readonly long id;
        private readonly string logName;
        private readonly DateTime creationTime;
        private readonly int managedThreadId;
        private readonly string threadName;
        private readonly string userName;
        private readonly string hostName;
        private readonly string message;
        private readonly LogLevel logLevel;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logName"></param>
        /// <param name="creationTime"></param>
        /// <param name="managedThreadId"></param>
        /// <param name="threadName"></param>
        /// <param name="userName"></param>
        /// <param name="hostName"></param>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        public LogEntry(
            long id,
            string logName,
            DateTime creationTime,
            int managedThreadId,
            string threadName,
            string userName,
            string hostName,
            string message,
            LogLevel logLevel)
        {
            this.id = id;
            this.logName = logName;
            this.creationTime = creationTime;
            this.managedThreadId = managedThreadId;
            this.threadName = threadName;
            this.userName = userName;
            this.hostName = hostName;
            this.message = message;
            this.logLevel = logLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Id
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string LogName
        {
            get
            {
                return this.logName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
                return this.creationTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ManagedThreadId
        {
            get
            {
                return this.managedThreadId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ThreadName
        {
            get
            {
                return this.threadName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string HostName
        {
            get
            {
                return this.hostName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LogLevel LogLevel
        {
            get
            {
                return this.logLevel;
            }
        }
    }
}