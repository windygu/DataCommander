namespace DataCommander.Foundation.Data.PTypes
{
    using System;
    using System.Data.SqlTypes;

    /// <summary>
    /// 
    /// </summary>
    public struct PDouble : INullable
    {
        private SqlDouble sql;
        private PValueType type;

        /// <summary>
        /// 
        /// </summary>
        public static readonly PDouble Null = new PDouble( PValueType.Null );

        /// <summary>
        /// 
        /// </summary>
        public static readonly PDouble Default = new PDouble( PValueType.Default );

        /// <summary>
        /// 
        /// </summary>
        public static readonly PDouble Empty = new PDouble( PValueType.Empty );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public PDouble( Double value )
        {
            this.sql = value;
            this.type = this.sql.IsNull ? PValueType.Null : PValueType.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public PDouble( SqlDouble value )
        {
            this.sql = value;
            this.type = value.IsNull ? PValueType.Null : PValueType.Value;
        }

        private PDouble( PValueType type )
        {
            this.type = type;
            this.sql = SqlDouble.Null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PDouble( Double value )
        {
            return new PDouble( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PDouble( SqlDouble value )
        {
            return new PDouble( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Double( PDouble value )
        {
            return (Double) value.sql;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Boolean operator ==( PDouble x, PDouble y )
        {
            Boolean isEqual = x.type == y.type;

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
        public static Boolean operator !=( PDouble x, PDouble y )
        {
            return !(x == y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PDouble Parse( String s, PValueType type )
        {
            PDouble sp;

            if (s == null || s.Length == 0)
            {
                sp = new PDouble( type );
            }
            else
            {
                sp = SqlDouble.Parse( s );
            }

            return sp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Boolean Equals( Object y )
        {
            Boolean equals = y is PDouble;

            if (equals)
            {
                equals = this == (PDouble) y;
            }

            return equals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Int32 GetHashCode()
        {
            Int32 hashCode = this.sql.GetHashCode();
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
        public Boolean IsNull
        {
            get
            {
                return this.type == PValueType.Null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsValue
        {
            get
            {
                return this.type == PValueType.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsEmpty
        {
            get
            {
                return this.type == PValueType.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Object Value
        {
            get
            {
                Object value;

                switch (this.type)
                {
                    case PValueType.Value:
                    case PValueType.Null:
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
                    this.sql = SqlDouble.Null;
                }
                else if (value == DBNull.Value)
                {
                    this.type = PValueType.Null;
                    this.sql = SqlDouble.Null;
                }
                else
                {
                    this.sql = (SqlDouble) value;
                    this.type = this.sql.IsNull ? PValueType.Null : PValueType.Value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return this.sql.ToString();
        }
    }
}