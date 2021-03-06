﻿using System.Collections;
using System.Collections.Generic;

namespace Foundation.Collections
{
    public sealed class EmptyEnumerable<T> : IEnumerable<T>
    {
        public static readonly EmptyEnumerable<T> Value = new EmptyEnumerable<T>();

        private EmptyEnumerable()
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return EmptyEnumerator<T>.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EmptyNonGenericEnumerator.Value;
        }
    }
}