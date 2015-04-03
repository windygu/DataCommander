namespace DataCommander.Foundation.Data.PTypes
{
    using System;
    using System.Data.SqlTypes;

    /// <summary>
    /// 
    /// </summary>
    public struct PDecimal : INullable
    {
        private SqlDecimal sql;
        private PValueType type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public PDecimal( decimal value )
        {
            this.sql = value;
            this.type = PValueType.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public PDecimal( SqlDecimal value )
        {
            this.sql = value;
            this.type = value.IsNull ? PValueType.Null : PValueType.Value;
        }

        private PDecimal( PValueType type )
        {
            this.type = type;
            this.sql = SqlDecimal.Null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PDecimal( decimal value )
        {
            return new PDecimal( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PDecimal( SqlDecimal value )
        {
            return new PDecimal( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator decimal( PDecimal value )
        {
            return (decimal) value.sql;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==( PDecimal x, PDecimal y )
        {
            bool isEqual = x.type == y.type;

            if (isEqual)
            {
                if (x.type == PValueType.Value)
                {
                    isEqual = x.sql.Value == y.sql.Value;
                }
            }

            return isEqual;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=( PDecimal x, PDecimal y )
        {
            return !(x == y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PDecimal Parse( string s, PValueType type )
        {
            PDecimal sp;

            if (string.IsNullOrEmpty( s ))
            {
                sp = new PDecimal( type );
            }
            else
            {
                sp = SqlDecimal.Parse( s );
            }

            return sp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Equals( object y )
        {
            bool equals = y is PDecimal;

            if (equals)
            {
                equals = this == (PDecimal) y;
            }

            return equals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = this.sql.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// 
        /// </summary>
        public PValueType ValueType
        {
            get
            {
                return this.type;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this.type == PValueType.Null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValue
        {
            get
            {
                return this.type == PValueType.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.type == PValueType.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object Value
        {
            get
            {
                object value;

                switch (this.type)
                {
                    case PValueType.Null:
                        value = DBNull.Value;
                        break;

                    case PValueType.Value:
                        value = this.sql;
                        break;

                    default:
                        value = null;
                        break;
                }

                return value;
            }

            set
            {
                if (value == null)
                {
                    this.type = PValueType.Default;
                    this.sql = SqlDecimal.Null;
                }
                else if (value == DBNull.Value)
                {
                    this.type = PValueType.Null;
                    this.sql = SqlDecimal.Null;
                }
                else
                {
                    this.sql = (SqlDecimal) value;
                    this.type = this.sql.IsNull ? PValueType.Null : PValueType.Value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.sql.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly PDecimal Null = new PDecimal( PValueType.Null );

        /// <summary>
        /// 
        /// </summary>
        public static readonly PDecimal Default = new PDecimal( PValueType.Default );

        /// <summary>
        /// 
        /// </summary>
        public static readonly PDecimal Empty = new PDecimal( PValueType.Empty );
    }
}