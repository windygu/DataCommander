﻿namespace Foundation.Data.LoggedDbConnection
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BeforeExecuteCommandEventArgs : LoggedEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public BeforeExecuteCommandEventArgs(LoggedDbCommandInfo command)
        {
            Command = command;
        }

        /// <summary>
        /// 
        /// </summary>
        public LoggedDbCommandInfo Command { get; }
    }
}