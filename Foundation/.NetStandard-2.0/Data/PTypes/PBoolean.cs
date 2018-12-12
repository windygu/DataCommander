﻿using System;
using System.Data.SqlTypes;

namespace Foundation.Data.PTypes
{
    public struct PBoolean : INullable
    {
        private SqlBoolean _sql;

        public static readonly PBoolean Null = new PBoolean(PValueType.Null);
        public static readonly PBoolean Default = new PBoolean(PValueType.Default);
        public static readonly PBoolean Empty = new PBoolean(PValueType.Empty);
        public static readonly PBoolean True = new PBoolean(true);
        public static readonly PBoolean False = new PBoolean(false);

        #region Constructors

        public PBoolean(bool value)
        {
            _sql = value;
            ValueType = PValueType.Value;
        }

        public PBoolean(SqlBoolean value)
        {
            _sql = value;
            ValueType = value.IsNull ? PValueType.Null : PValueType.Value;
        }

        private PBoolean(PValueType type)
        {
            ValueType = type;
            _sql = SqlBoolean.Null;
        }

        #endregion

        public static implicit operator PBoolean(bool value) => new PBoolean(value);

        public static implicit operator PBoolean(bool? value)
        {
            PBoolean target;
            if (value == null)
                target = Null;
            else if (value.Value)
                target = True;
            else
                target = False;

            return target;
        }

        public static implicit operator PBoolean(SqlBoolean value) => new PBoolean(value);
        public static implicit operator bool(PBoolean value) => (bool) value._sql;

        public static bool operator ==(PBoolean x, PBoolean y)
        {
            var isEqual = x.ValueType == y.ValueType;
            if (isEqual)
            {
                if (x.ValueType == PValueType.Value)
                {
                    isEqual = x._sql.Value == y._sql.Value;
                }
            }

            return isEqual;
        }

        public static bool operator !=(PBoolean x, PBoolean y) => !(x == y);

        /// <summary>
        /// Converts the specified <see cref="System.String"/> representation of a logical value
        /// to its <see cref="PBoolean"/> equivalent.
        /// </summary>
        /// <param name="s">
        /// The <see cref="System.String"/> to be converted. 
        /// </param>
        /// <param name="type"></param>
        /// <returns>
        /// An <see cref="PBoolean"/> structure containing the parsed value.
        /// </returns>
        public static PBoolean Parse(string s, PValueType type)
        {
            PBoolean sp;

            if (string.IsNullOrEmpty(s))
                sp = new PBoolean(type);
            else
                sp = SqlBoolean.Parse(s);

            return sp;
        }

        public override bool Equals(object y)
        {
            var equals = y is PBoolean;

            if (equals)
                equals = this == (PBoolean) y;

            return equals;
        }

        public override int GetHashCode()
        {
            var hashCode = _sql.GetHashCode();
            return hashCode;
        }

        public PValueType ValueType { get; private set; }

        public bool IsNull => ValueType == PValueType.Null;
        public bool IsValue => ValueType == PValueType.Value;
        public bool IsEmpty => ValueType == PValueType.Empty;

        public object Value
        {
            get
            {
                object value;

                switch (ValueType)
                {
                    case PValueType.Value:
                    case PValueType.Null:
                        value = _sql;
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
                    ValueType = PValueType.Default;
                    _sql = SqlBoolean.Null;
                }
                else if (value == DBNull.Value)
                {
                    ValueType = PValueType.Null;
                    _sql = SqlBoolean.Null;
                }
                else
                {
                    _sql = (SqlBoolean) value;
                    ValueType = _sql.IsNull ? PValueType.Null : PValueType.Value;
                }
            }
        }

        public bool IsTrue => _sql.IsTrue;
        public bool IsFalse => _sql.IsFalse;
        public override string ToString() => _sql.ToString();
    }
}