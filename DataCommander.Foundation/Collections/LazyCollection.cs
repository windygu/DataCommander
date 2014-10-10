﻿namespace DataCommander.Foundation.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyCollection<T> : ICollection<T>
    {
        #region Private Fields

        private Func<ICollection<T>> createCollection;
        private ICollection<T> collection;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createCollection"></param>
        public LazyCollection( Func<ICollection<T>> createCollection )
        {
            this.Initialize( createCollection );
        }

        /// <summary>
        /// 
        /// </summary>
        protected LazyCollection()
        {
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createCollection"></param>
        protected void Initialize( Func<ICollection<T>> createCollection )
        {
            Contract.Requires( createCollection != null );

            this.createCollection = createCollection;
        }

        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public ICollection<T> InnerCollection
        {
            get
            {
                return this.collection;
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add( T item )
        {
            this.GetCollection();
            this.collection.Add( item );
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (this.collection != null)
            {
                this.collection.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Boolean Contains( T item )
        {
            Boolean contains;

            if (this.collection != null)
            {
                contains = this.collection.Contains( item );
            }
            else
            {
                contains = false;
            }

            return contains;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo( T[] array, Int32 arrayIndex )
        {
            if (this.collection != null)
            {
                this.collection.CopyTo( array, arrayIndex );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 Count
        {
            get
            {
                Int32 count;

                if (this.collection != null)
                {
                    count = this.collection.Count;
                }
                else
                {
                    count = 0;
                }

                return count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsReadOnly
        {
            get
            {
                this.GetCollection();
                Boolean isReadOnly = isReadOnly = this.collection.IsReadOnly;
                return isReadOnly;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Boolean Remove( T item )
        {
            Boolean removed;

            if (this.collection != null)
            {
                removed = this.collection.Remove( item );
            }
            else
            {
                removed = false;
            }

            return removed;
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> enumerator;

            if (this.collection != null)
            {
                enumerator = this.collection.GetEnumerator();
            }
            else
            {
                enumerator = new Enumerator();
            }

            return enumerator;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<T> enumerator = this.GetEnumerator();
            return enumerator;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void GetCollection()
        {
            if (this.collection == null)
            {
                this.collection = this.createCollection();
                Contract.Assert( this.collection != null );
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private sealed class Enumerator : IEnumerator<T>
        {
            #region IEnumerator<T> Members

            T IEnumerator<T>.Current
            {
                get
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            Object IEnumerator.Current
            {
                get
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            Boolean IEnumerator.MoveNext()
            {
                return false;
            }

            void IEnumerator.Reset()
            {
            }

            #endregion
        }
    }
}