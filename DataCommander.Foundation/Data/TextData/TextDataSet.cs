﻿namespace DataCommander.Foundation.Data
{
    using System;
	using System.Diagnostics;

	/// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Name = {name}")]
    public sealed class TextDataSet
    {
        private readonly String name;
        private readonly TextDataSetTableCollection tables = new TextDataSetTableCollection();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public TextDataSet(String name)
        {
            this.name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public String Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TextDataSetTableCollection Tables
        {
            get
            {
                return this.tables;
            }
        }
    }
}