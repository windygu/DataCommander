namespace DataCommander.Foundation.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public class AsyncTextWriter
    {
        #region Private Fields

        private readonly TextWriter textWriter;
        private readonly List<AsyncTextWriterListItem> list = new List<AsyncTextWriterListItem>();
        private readonly object syncObject = new object();
        private readonly ManualResetEvent waitHandle = new ManualResetEvent(false);
        private RegisteredWaitHandle registeredWaitHandle;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textWriter"></param>
        public AsyncTextWriter(TextWriter textWriter)
        {
#if CONTRACTS_FULL
            Contract.Requires<ArgumentNullException>(textWriter != null);
#endif

            this.textWriter = textWriter;
        }

        private void Flush()
        {
            var sb = new StringBuilder();

            while (this.list.Count > 0)
            {
                AsyncTextWriterListItem[] items;
                lock (this.list)
                {
                    var count = this.list.Count;
                    items = new AsyncTextWriterListItem[count];
                    this.list.CopyTo(items);
                    this.list.Clear();
                }

                for (var i = 0; i < items.Length; i++)
                {
                    items[i].AppendTo(sb);
                }
            }

            this.textWriter.Write(sb);
            this.textWriter.Flush();
        }

        private void Unregister()
        {
            lock (this.syncObject)
            {
                if (this.registeredWaitHandle != null)
                {
                    ////log.Write(LogLevel.Trace,"Unregister...");
                    var succeeded = this.registeredWaitHandle.Unregister(null);
                    this.registeredWaitHandle = null;
                    ////log.Write(LogLevel.Trace,"Unregister succeeded.");
                }
            }
        }

        private void Callback(object state, bool timedOut)
        {
            if (this.list.Count > 0)
            {
                this.Flush();
            }
            else
            {
                this.Unregister();
            }
        }

        private void Write(AsyncTextWriterListItem item)
        {
            lock (this.list)
            {
                this.list.Add(item);
            }

            const int timeout = 10000; // 10 seconds

            lock (this.syncObject)
            {
                if (this.registeredWaitHandle == null)
                {
                    // log.Write("ThreadPool.RegisterWaitForSingleObject",LogLevel.Trace);
                    this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(this.waitHandle, this.Callback,
                        null, timeout, false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value)
        {
            var item = new AsyncTextWriterListItem(DefaultFormatter.Instance, value);
            this.Write(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="args"></param>
        public void Write(IFormatter formatter, params object[] args)
        {
            var item = new AsyncTextWriterListItem(formatter, args);
            this.Write(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            this.Unregister();
            this.Flush();
            this.textWriter.Close();
        }
    }
}