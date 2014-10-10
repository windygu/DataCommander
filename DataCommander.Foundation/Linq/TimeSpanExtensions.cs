﻿namespace DataCommander.Foundation.Linq
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static Double GetTotalMicroseconds( this TimeSpan timeSpan )
        {
            return timeSpan.Ticks * 0.1;
        }
    }
}