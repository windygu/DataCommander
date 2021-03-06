﻿using System;
using Foundation.Assertions;

namespace Foundation
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    public sealed class ArgumentEqualsSelection<TArgument> where TArgument : IEquatable<TArgument>
    {
        private readonly TArgument argument;
        private bool selected;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        public ArgumentEqualsSelection(TArgument argument)
        {
            this.argument = argument;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public ArgumentEqualsSelection<TArgument> IfArgumentEquals(TArgument other, Action action)
        {
            Assert.IsNotNull(action);

            if (!selected)
            {
                selected = argument.Equals(other);
                if (selected)
                {
                    action();
                }
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Else(Action action)
        {
            Assert.IsNotNull(action);

            if (!selected)
                action();
        }
    }
}