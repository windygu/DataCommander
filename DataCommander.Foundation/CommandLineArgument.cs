﻿namespace DataCommander.Foundation
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CommandLineArgument
    {
        private readonly int index;
        private readonly string name;
        private readonly string value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public CommandLineArgument(
            int index,
            string name,
            string value)
        {
            this.index = index;
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Index => this.index;

        /// <summary>
        /// 
        /// </summary>
        public string Name => this.name;

        /// <summary>
        /// 
        /// </summary>
        public string Value => this.value;
    }
}