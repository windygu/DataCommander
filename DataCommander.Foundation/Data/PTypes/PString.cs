namespace DataCommander.Foundation.Data.PTypes
{
    using System;
    using System.Data.SqlTypes;
    using System.Diagnostics;

    /// <summary>
    /// 
    /// </summary>
    public struct PString : INullable
    {
        private SqlString sql;
        private PValueType type;

        /// <summary>
        /// 
        /// </summary>
        public static readonly PString Null = new PString( PValueType.Null );

        /// <summary>
        /// 
        /// </summary>
        public static readonly PString Default = new PString( PValueType.Default );

        /// <summary>
        /// 
        /// </summary>
        public static readonly PString Empty = new PString( PValueType.Empty );

        private PString( PValueType type )
        {
            this.type = type;
            this.sql = SqlString.Null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public PString( Char value )
        {
            this.sql = value.ToString();
            this.type = PValueType.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        [DebuggerStepThrough]
        public PString( string value )
        {
            this.sql = value;
            this.type = PValueType.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public PString( SqlString value )
        {
            this.sql = value;
            this.type = value.IsNull ? PValueType.Null : PValueType.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PString( Char value )
        {
            return new PString( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static implicit operator PString( string value )
        {
            return new PString( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PString( SqlString value )
        {
            return new PString( value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator string( PString value )
        {
            return (string) value.sql;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==( PString x, PString y )
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
        public static bool operator !=( PString x, PString y )
        {
            return !(x == y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PString Parse( string s, PValueType type )
        {
            PString sp;

            if (s == null || s.Length == 0)
            {
                sp = new PString( type );
            }
            else
            {
                sp = new PString( s );
            }

            return sp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            bool equals = obj is PString;

            if (equals)
            {
                equals = this == (PString) obj;
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
        public PValueType ValueType => this.type;

        /// <summary>
        /// 
        /// </summary>
        public bool IsNull => this.type == PValueType.Null;

        /// <summary>
        /// 
        /// </summary>
        public bool IsValue => this.type == PValueType.Value;

        /// <summary>
        /// 
        /// </summary>
        public bool IsEmpty => this.type == PValueType.Empty;

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
                    this.sql = SqlString.Null;
                }
                else if (value == DBNull.Value)
                {
                    this.type = PValueType.Null;
                    this.sql = SqlString.Null;
                }
                else
                {
                    this.sql = (SqlString) value;
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
    }
}