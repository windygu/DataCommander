﻿namespace DataCommander.Foundation
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Conjunction<T> : PredicateClass<T>
    {
        private readonly PredicateClass<T> x;
        private readonly PredicateClass<T> y;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Conjunction( PredicateClass<T> x, PredicateClass<T> y )
        {
            Contract.Requires( x != null );
            Contract.Requires( y != null );

            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override Boolean Evaluate( T value )
        {
            return this.x.Evaluate( value ) && this.y.Evaluate( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return "(" + this.x + " and " + this.y + ")";
        }
    }
}