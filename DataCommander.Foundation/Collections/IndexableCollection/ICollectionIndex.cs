﻿namespace DataCommander.Foundation.Collections.IndexableCollection
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectionIndex<T> : ICollection<T>
    {
        /// <summary>
        /// 
        /// </summary>
        string Name
        {
            get;
        }
    }
}