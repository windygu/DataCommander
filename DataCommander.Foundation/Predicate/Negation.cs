namespace DataCommander.Foundation
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Negation<T> : PredicateClass<T>
    {
        private readonly PredicateClass<T> predicate;

        /// <summary>        
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        public Negation(PredicateClass<T> predicate)
        {
            Contract.Requires(predicate != null);

            this.predicate = predicate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override Boolean Evaluate(T value)
        {
            return !this.predicate.Evaluate(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return "not (" + this.predicate + ")";
        }
    }
}