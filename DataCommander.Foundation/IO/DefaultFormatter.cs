﻿namespace DataCommander.Foundation.IO
{
    using System;
    using System.Text;

    internal sealed class DefaultFormatter : IFormatter
    {
        private static readonly DefaultFormatter instance = new DefaultFormatter();

        public static DefaultFormatter Instance
        {
            get
            {
                return instance;
            }
        }

        void IFormatter.AppendTo( StringBuilder sb, Object[] args )
        {
            sb.Append( args[ 0 ] );
        }
    }
}