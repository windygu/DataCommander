﻿using System.Data.OleDb;

namespace DataCommander.Providers.OleDb
{
    internal sealed class ConnectionStringBuilder : IDbConnectionStringBuilder
    {
        private readonly OleDbConnectionStringBuilder oleDbConnectionStringBuilder = new OleDbConnectionStringBuilder();

        string IDbConnectionStringBuilder.ConnectionString
        {
            get => oleDbConnectionStringBuilder.ConnectionString;

            set => oleDbConnectionStringBuilder.ConnectionString = value;
        }

        bool IDbConnectionStringBuilder.IsKeywordSupported(string keyword)
        {
            return true;
        }

        void IDbConnectionStringBuilder.SetValue(string keyword, object value)
        {
            oleDbConnectionStringBuilder[keyword] = value;
        }

        bool IDbConnectionStringBuilder.TryGetValue(string keyword, out object value)
        {
            return oleDbConnectionStringBuilder.TryGetValue(keyword, out value);
        }
    }
}