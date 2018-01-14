using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.Core.Shared.Model.Groups
{
    public struct GroupBuilder<T>
    {
        public Func<T, object> OrderBy { get; private set; }
        public Func<T, string> GroupBy { get; private set; }
        public IComparer<Group<T>> GroupComparer { get; private set; }
        public IComparer<T> ItemComparer { get; private set; }
        public bool Ascending { get; private set; }

        public GroupBuilder(Func<T, object> orderBy, Func<T, string> groupBy, Func<Group<T>, Group<T>, int> compareGroup, Func<T, T, int> compareItem, bool ascending = true)
            : this(orderBy, groupBy, new GenericComparer<Group<T>>(compareGroup), new GenericComparer<T>(compareItem), ascending)
        {
        }

        public GroupBuilder(Func<T, object> orderBy, Func<T, string> groupBy, IComparer<Group<T>> groupComparer, IComparer<T> itemComparer, bool ascending = true)
            : this()
        {
            if (orderBy == null)
                throw new ArgumentNullException("orderBy");
            if (groupBy == null)
                throw new ArgumentNullException("groupBy");
            if (groupComparer == null)
                throw new ArgumentNullException("groupComparer");
            if (itemComparer == null)
                throw new ArgumentNullException("itemComparer");

            this.OrderBy = orderBy;
            this.GroupBy = groupBy;
            this.GroupComparer = groupComparer;
            this.ItemComparer = itemComparer;
            this.Ascending = ascending;
        }
    }
}