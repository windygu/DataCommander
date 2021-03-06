﻿namespace Foundation.Linq
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct IndexedItem<T>
    {
        #region Private Fields

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public IndexedItem(int index, T value)
        {
            Index = index;
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 
        /// </summary>
        public T Value { get; }
    }
}