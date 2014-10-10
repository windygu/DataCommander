namespace DataCommander.Foundation.Threading
{
    using System;
    using System.Collections;
    using System.Diagnostics.Contracts;
    using System.Threading;

    /// <summary>
    /// Asynchronous producer/consumer queue handler class.
    /// </summary>
    public class AsyncQueue
    {
        private String name;
        private IAsyncQueue asyncQueue;
        private WorkerThreadCollection consumers = new WorkerThreadCollection();
        private Queue queue = new Queue();
        private AutoResetEvent queueEvent = new AutoResetEvent( false );

        private sealed class ConsumerThread
        {
            private AsyncQueue queue;
            private WorkerThread thread;
            private IConsumer consumer;

            public ConsumerThread(
                AsyncQueue queue,
                Int32 id,
                ThreadPriority priority )
            {
                this.queue = queue;
                this.thread = new WorkerThread( this.ThreadStart );
                this.thread.Name = String.Format( "Consumer({0},{1})", queue.name, id );
                this.thread.Priority = priority;
                this.consumer = this.queue.asyncQueue.CreateConsumer( this.thread, id );
            }

            private void ThreadStart()
            {
                Object state = this.consumer.Enter();

                try
                {
                    while (!this.thread.IsStopRequested)
                    {
                        this.queue.Dequeue( this );
                    }
                }
                finally
                {
                    this.consumer.Exit( state );
                }
            }

            public void Consume( Object item )
            {
                this.consumer.Consume( item );
            }

            public WorkerThread Thread
            {
                get
                {
                    return this.thread;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected AsyncQueue()
        {
        }

        /// <summary>
        /// Inherited class must call this initializer method first.
        /// </summary>
        /// <param name="name">The name of the <see cref="AsyncQueue"/></param>
        /// <param name="asyncQueue">The <see cref="IAsyncQueue"/> implementation</param>
        /// <param name="consumerCount">Number of consumers</param>
        /// <param name="priority">priority of consumer threads</param>
        protected void Initialize(
            String name,
            IAsyncQueue asyncQueue,
            Int32 consumerCount,
            ThreadPriority priority )
        {
            Contract.Requires( consumerCount > 0 );

            this.name = name;
            this.asyncQueue = asyncQueue;

            for (Int32 id = 0; id < consumerCount; id++)
            {
                ConsumerThread consumerThread = new ConsumerThread( this, id, priority );
                this.consumers.Add( consumerThread.Thread );
            }
        }

        /// <summary>
        /// Calls <see cref="Initialize"/> method.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="asyncQueue"></param>
        /// <param name="consumerCount"></param>
        /// <param name="priority"></param>
        public AsyncQueue(
            String name,
            IAsyncQueue asyncQueue,
            Int32 consumerCount,
            ThreadPriority priority )
        {
            this.Initialize( name, asyncQueue, consumerCount, priority );
        }

        /// <summary>
        /// Adds an item to the queue.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue( Object item )
        {
            Contract.Requires( item != null );

            lock (this.queue)
            {
                this.queue.Enqueue( item );
            }

            this.queueEvent.Set();
        }

        private Object Dequeue()
        {
            Object item = null;

            if (this.queue.Count > 0)
            {
                lock (this.queue)
                {
                    if (this.queue.Count > 0)
                    {
                        item = this.queue.Dequeue();
                    }
                }
            }

            return item;
        }

        private void Consume( ConsumerThread consumerThread, Object item )
        {
            Contract.Requires( consumerThread != null );

            AsyncQueueConsumeEventArgs args = new AsyncQueueConsumeEventArgs( item );
            var eventHandler = this.asyncQueue.BeforeConsume;

            if (eventHandler != null)
            {
                eventHandler( this, args );
            }

            consumerThread.Consume( item );

            eventHandler = this.asyncQueue.AfterConsume;

            if (eventHandler != null)
            {
                eventHandler( this, args );
            }
        }

        private void Dequeue( ConsumerThread consumerThread )
        {
            WorkerThread thread = consumerThread.Thread;
            WaitHandle[] waitHandles = { thread.StopRequest, this.queueEvent };

            while (!thread.IsStopRequested)
            {
                Object item = this.Dequeue();

                if (item != null)
                {
                    this.Consume( consumerThread, item );
                }
                else
                {
                    WaitHandle.WaitAny( waitHandles );
                }
            }
        }

        /// <summary>
        /// Gets the number of unconsumed items (queued items).
        /// </summary>
        public Int32 Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        /// <summary>
        /// Gets the consumer thread list.
        /// </summary>
        public WorkerThreadCollection Consumers
        {
            get
            {
                return this.consumers;
            }
        }
    }
}