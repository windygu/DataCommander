namespace DataCommander.Foundation.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using DataCommander.Foundation.Configuration;

    /// <exclude/>
    public static class StringHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="colWidth"></param>
        /// <param name="alignRight"></param>
        /// <returns></returns>
        public static string FormatColumn(
            string col,
            int colWidth,
            bool alignRight )
        {
            var length = col != null ? col.Length : 0;
            var spaceLen = colWidth - length;
            string formatted;

            if (spaceLen >= 0)
            {
                var space = new string( ' ', spaceLen );

                if (alignRight)
                {
                    formatted = space + col;
                }
                else
                {
                    formatted = col + space;
                }
            }
            else
            {
                formatted = col.Substring( 0, colWidth );
            }

            return formatted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index"></param>
        /// <param name="ch"></param>
        public static unsafe void SetChar( string s, int index, Char ch )
        {
            Contract.Requires( index >= 0 );
            Contract.Requires( index < s.Length );

            fixed (Char* p = s)
            {
                p[ index ] = ch;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public static unsafe void ToLower( string s )
        {
            fixed (Char* pfixed = s)
            {
                for (var p = pfixed; *p != 0; p++)
                {
                    *p = Char.ToLower( *p, CultureInfo.CurrentCulture );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public static unsafe void ToUpper( string s )
        {
            fixed (Char* pfixed = s)
            {
                for (var p = pfixed; *p != 0; p++)
                {
                    *p = Char.ToUpper( *p, CultureInfo.CurrentCulture );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public static void WriteMethod(
            TextWriter textWriter,
            object obj,
            string methodName,
            params object[] parameters )
        {
            Contract.Requires( textWriter != null );
            Contract.Requires( obj != null );

            var type = obj.GetType();
            var methodInfo = type.GetMethod( methodName );
            var parameterInfos = methodInfo.GetParameters();

            var typeName = TypeNameCollection.GetTypeName( methodInfo.ReturnType );

            var line = typeName + " " + methodName + "(" + Environment.NewLine;

            var length = Math.Min( parameters.Length, parameterInfos.Length );

            for (var i = 0; i < length; i++)
            {
                typeName = TypeNameCollection.GetTypeName( parameterInfos[ i ].ParameterType );

                line +=
                    "  " + typeName + " " +
                    parameterInfos[ i ].Name + " = " +
                    parameters[ i ];

                if (i < length - 1)
                {
                    line += "," + Environment.NewLine;
                }
            }

            line += ')';

            textWriter.Write( line );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public static bool ParseBoolean( string value, bool nullValue )
        {
            var b = value != null && value.Length > 0 ? bool.Parse( value ) : nullValue;
            return b;
        }
    }
}