using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Chartreuse.Today.Core.Shared.Tools.Collection
{
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public SortableObservableCollection(IEnumerable<T> items)
            : base(items)
        {
        }

        public SortableObservableCollection()
        {
        }

        public Dictionary<int, int> Sort(Func<T, T, int> compare, bool ascending = true)
        {
            return this.Sort(new GenericComparer<T>(compare), ascending);
        }

        public Dictionary<int, int> Sort(IComparer<T> comparer, bool ascending = true)
        {
            List<T> originalList = this.Items.ToList();
            List<T> newList = (List<T>)this.Items;

            if (ascending)
                newList.Sort(comparer);
            else
                newList.Sort((x, y) => comparer.Compare(y, x));

            var change = new Dictionary<int, int>();
            for (int i = 0; i < newList.Count; i++)
            {
                if (!newList[i].Equals(originalList[i]))
                {
                    // position of this item has changed
                    // add an entry where the key is the original index, and the value the new index
                    int originalIndex = originalList.IndexOf(newList[i]);
                    if (originalIndex != i)
                        change.Add(originalIndex, i);
                }
            }

            if (change.Count > 0)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, change, change, 0));
            }

            return change;
        }
    }
}
