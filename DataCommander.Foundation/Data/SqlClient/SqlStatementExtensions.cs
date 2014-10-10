﻿namespace DataCommander.Foundation.Data.SqlClient
{
    using System;
    using System.Data.SqlTypes;
    using System.Globalization;

    /// <summary>
    /// 
    /// </summary>
    public static class SqlStatementExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        private const Byte True = 1;

        /// <summary>
        /// 
        /// </summary>
        private const Byte False = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Byte ToTSqlBit( this Boolean source )
        {
            return source ? True : False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToTSqlDateTime( this DateTime source )
        {
            TimeSpan timeOfDay = source.TimeOfDay;
            String format;

            if (timeOfDay.TotalMilliseconds == 0)
            {
                format = "yyyyMMdd";
            }
            else
            {
                if (source.Millisecond == 0)
                {
                    format = "yyyyMMdd HH:mm:ss";
                }
                else
                {
                    format = "yyyyMMdd HH:mm:ss.fff";
                }
            }

            return string.Format( "'{0}'", source.ToString( format, CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Converts a <see cref="System.Decimal"/> value to Microsoft SQL Server Decimal String.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToTSqlDecimal( this decimal source )
        {
            return source.ToString( NumberFormatInfo.InvariantInfo );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToTSqlInt( this Int32? source )
        {
            return source != null ? source.ToString() : SqlNull.NullString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToTSqlVarChar( this string source )
        {
            string target;
            if (source != null)
            {
                target = "'" + source.Replace( "'", "''" ) + "'";
            }
            else
            {
                target = SqlNull.NullString;
            }
            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static String ToTSqlNVarChar( this String source )
        {
            string target;
            if (source != null)
            {
                target = "N'" + source.Replace( "'", "''" ) + "'";
            }
            else
            {
                target = SqlNull.NullString;
            }
            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SqlDateTime ToSqlDateTime( this DateTime? value )
        {
            SqlDateTime returnValue;

            if (value != null)
            {
                returnValue = value.Value;
            }
            else
            {
                returnValue = SqlDateTime.Null;
            }

            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SqlInt32 ToSqlInt32( this Int32? value )
        {
            SqlInt32 returnValue;

            if (value != null)
            {
                returnValue = value.Value;
            }
            else
            {
                returnValue = SqlInt32.Null;
            }

            return returnValue;
        }
    }
}