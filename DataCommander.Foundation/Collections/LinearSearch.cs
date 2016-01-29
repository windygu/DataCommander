﻿namespace DataCommander.Foundation.Collections
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// 
    /// </summary>
    public static class LinearSearch
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minIndex"></param>
        /// <param name="maxIndex"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int IndexOf(int minIndex, int maxIndex, Func<int, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(predicate != null);

            int index = -1;

            while (minIndex <= maxIndex)
            {
                if (predicate(minIndex))
                {
                    index = minIndex;
                    break;
                }

                minIndex++;
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minIndex"></param>
        /// <param name="maxIndex"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int LastIndexOf(int minIndex, int maxIndex, Func<int, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(predicate != null);

            int index = -1;

            while (minIndex <= maxIndex)
            {
                if (predicate(maxIndex))
                {
                    index = maxIndex;
                    break;
                }

                maxIndex--;
            }

            return index;
        }
    }
}