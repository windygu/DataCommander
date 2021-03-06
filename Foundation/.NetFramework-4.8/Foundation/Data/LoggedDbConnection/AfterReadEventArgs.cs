﻿namespace Foundation.Data.LoggedDbConnection
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AfterReadEventArgs : LoggedEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowCount"></param>
        public AfterReadEventArgs(int rowCount)
        {
            RowCount = rowCount;
        }

        /// <summary>
        /// 
        /// </summary>
        public int RowCount { get; }
    }
}