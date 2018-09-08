﻿using System;
using Foundation.Assertions;
using Foundation.Diagnostics.Contracts;

namespace Foundation.Text
{
    public sealed class StringTableRow
    {
        private readonly string[] _cells;

        internal StringTableRow(StringTable table)
        {
            Assert.IsNotNull(table);

            Table = table;
            _cells = new string[table.Columns.Count];
        }

        public StringTable Table { get; }

        public string this[int columnIndex]
        {
            get
            {
                FoundationContract.Requires<ArgumentException>(0 <= columnIndex && columnIndex < Table.Columns.Count);
                return _cells[columnIndex];
            }

            set
            {
                FoundationContract.Requires<ArgumentException>(0 <= columnIndex && columnIndex < Table.Columns.Count);
                _cells[columnIndex] = value;
            }
        }
    }
}