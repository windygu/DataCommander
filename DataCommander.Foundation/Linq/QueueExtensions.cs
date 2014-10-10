﻿namespace DataCommander.Foundation.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// 
    /// </summary>
    public static class QueueExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static T DequeueTail<T>( this Queue<T> queue )
        {
            Contract.Requires<ArgumentNullException>( queue != null );
            Contract.Requires( queue.Count > 0 );

            var array = new T[queue.Count];
            queue.CopyTo( array, 0 );
            queue.Clear();
            Int32 last = array.Length - 1;
            for (Int32 i = 0; i < last; i++)
            {
                queue.Enqueue( array[ i ] );
            }

            return array[ last ];
        }
    }
}