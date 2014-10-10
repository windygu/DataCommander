﻿namespace DataCommander.Foundation.Threading.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    /// <summary>
    /// 
    /// </summary>
    public class WorkerTask
    {
        private Task task;
        private TaskInfo taskInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="taskCreationOptions"></param>
        /// <param name="name"></param>
        public WorkerTask(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions taskCreationOptions, string name)
        {
            var response = TaskMonitor.CreateTask(action, state, cancellationToken, taskCreationOptions, name);
            this.task = response.Task;
            this.taskInfo = response.TaskInfo;
        }
    }
}