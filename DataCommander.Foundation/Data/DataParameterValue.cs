﻿namespace DataCommander.Foundation.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct DataParameterValue<T> : IDataParameterValue<T>
    {
        private readonly T value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public DataParameterValue( T value )
        {
            this.Type = DataParameterValueType.Value;
            this.value = value;
        }

        private DataParameterValue( DataParameterValueType type )
        {
            this.Type = type;
            this.value = default( T );
        }

        /// <summary>
        /// 
        /// </summary>
        public static DataParameterValue<T> Default { get; } = new DataParameterValue<T>( DataParameterValueType.Default );

        /// <summary>
        /// 
        /// </summary>
        public static DataParameterValue<T> Null { get; } = new DataParameterValue<T>( DataParameterValueType.Null );

        /// <summary>
        /// 
        /// </summary>
        public static DataParameterValue<T> Void { get; } = new DataParameterValue<T>( DataParameterValueType.Void );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator DataParameterValue<T>( T value )
        {
            return new DataParameterValue<T>( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator T( DataParameterValue<T> value )
        {
            return value.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        public DataParameterValueType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public T Value
        {
            get
            {
#if CONTRACTS_FULL
                Contract.Assert( this.Type == DataParameterValueType.Value );
#endif

                return this.value;
            }
        }

        object IDataParameterValue.ValueObject => this.Value;
    }
}