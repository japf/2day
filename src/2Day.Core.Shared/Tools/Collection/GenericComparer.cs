using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Tools.Collection
{
    public class GenericComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> compare;

        public GenericComparer(Func<T, T, int> compare)
        {
            if (compare == null)
                throw new ArgumentNullException("compare");

            this.compare = compare;
        }

        public int Compare(T x, T y)
        {
            return this.compare(x, y);
        }
    }
}