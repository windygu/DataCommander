﻿#if FOUNDATION_3_5
namespace DataCommander.Foundation.Caching
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface ICacheItem
    {
        /// <summary>
        /// 
        /// </summary>
        String Key
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan SlidingExpiration
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Object GetValue();
    }
}
#endif