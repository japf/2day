using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model.Groups;

namespace Chartreuse.Today.Core.Shared.Tools.Collection
{
    public class SmartCollection<T> : INotifyPropertyChanged
        where T : class
    {
        private readonly IEnumerable<T> content;
        private readonly object owner;
        private readonly Func<T, object> getGroupOwner;
        private readonly List<T> filterCache;
        private readonly SortableObservableCollection<Group<T>> groups;

        private GroupBuilder<T> groupBuilder;
        private Predicate<T> filter;
        private int count;
        
        public Predicate<T> Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                if (this.filter != value)
                {
                    this.filter = value;
                    if (this.filter == null)
                        this.filter = (item) => true;

                    this.Rebuild();
                }
            }
        }

        public GroupBuilder<T> GroupBuilder
        {
            get
            {
                return this.groupBuilder;
            }
            set
            {
                if (!this.groupBuilder.Equals(value))
                {
                    this.groupBuilder = value;
                    this.Rebuild();
                }
            }
        }

        public ObservableCollection<Group<T>> Items
        {
            get
            {
                return this.groups;
            }
        }

        /// <summary>
        /// Gets a value indicating whehter the collection is currently empty (eg. number of groups is 0) or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.groups.Count == 0;
            }
        }
        
        public int Count
        {
            get { return this.count; }
        }

        public object Owner
        {
            get { return this.owner; }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public SmartCollection(IEnumerable<T> content, GroupBuilder<T> groupBuilder, Predicate<T> filter, object owner = null, Func<T, object> getGroupOwner = null)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            if (filter == null)
                throw new ArgumentNullException("filter");

            this.content = content;
            this.owner = owner;
            this.getGroupOwner = getGroupOwner;
            this.filterCache = new List<T>();
            this.groupBuilder = groupBuilder;
            this.filter = filter;
            this.groups = new SortableObservableCollection<Group<T>>();

            this.Rebuild();
        }

        public bool Contains(T item)
        {
            return this.groups.Any(g => g.Contains(item));
        }

        public void Clear()
        {
            this.groups.Clear();
            this.OnContentPropertiesChanged();
        }
        
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (!this.ItemMatchFilter(item))
            {
                this.filterCache.Add(item);
                return;
            }

            string groupId = this.groupBuilder.GroupBy(item);

            // find a potential matching group
            var group = this.groups.FirstOrDefault(g => g.Title == groupId);

            if (group != null)
            {
                this.InsertWhereAppropriate(group, item);
            }
            else
            {
                // create a dedicated group
                group = new Group<T>(groupId, this.GetGroupOwner(item)) { item };

                // add the new group
                this.InsertGroupWhereAppropriate(group);
            }

            this.count++;
            this.OnContentPropertiesChanged();
        }

        private object GetGroupOwner(T item)
        {
            if (item == null || this.getGroupOwner == null)
                return this.owner;
            else
                return this.getGroupOwner(item);
        }

        public void Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (this.filterCache.Contains(item))
                this.filterCache.Remove(item);

            // find the associated group
            var group = this.groups.FirstOrDefault(g => g.Contains(item));
            if (group != null)
            {
                // remove this item from the group
                group.Remove(item);
                this.EnsureGroupIsNeeded(group);
                
                this.count--;
                this.OnContentPropertiesChanged();
            }
        }

        public void Rebuild()
        {
            var query = this.content
                .OrderBy(this.groupBuilder.OrderBy)
                .GroupBy(this.groupBuilder.GroupBy).ToList();

            this.groups.Clear();
            this.filterCache.Clear();
            this.count = 0;

            foreach (var items in query)
            {
                var group = new Group<T>(items.Key, this.GetGroupOwner(items.FirstOrDefault()));
                foreach (T item in items)
                {
                    if (!this.ItemMatchFilter(item))
                    {
                        if (!this.filterCache.Contains(item))
                            this.filterCache.Add(item);
                        continue;
                    }

                    this.count++;

                    // add it to the group
                    this.InsertWhereAppropriate(group, item);
                }

                if (group.Count > 0)
                {
                    this.InsertGroupWhereAppropriate(group);
                }
            }

            this.OnContentPropertiesChanged();
        }

        public void InvalidateItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (!this.filterCache.Contains(item) && !this.Contains(item))
                return;

            string groupId = this.groupBuilder.GroupBy(item);

            bool itemMatchFilter = this.ItemMatchFilter(item);

            if (!itemMatchFilter && !this.filterCache.Contains(item))
            {
                var group = this.groups.FirstOrDefault(g => g.Contains(item));
                if (group != null)
                {
                    group.Remove(item);
                    this.EnsureGroupIsNeeded(group);

                    this.filterCache.Add(item);

                    this.count--;
                }
            }
            else if (!itemMatchFilter && this.filterCache.Contains(item))
            {

            }
            else
            {
                this.filterCache.Remove(item);

                // find the group which owns this item
                var group = this.groups.FirstOrDefault(g => g.Contains(item));

                // check if the item should move to another group
                if (group != null && groupId != group.Title)
                {
                    // remove the item from the group
                    group.Remove(item);
                    this.EnsureGroupIsNeeded(group);

                    this.count--;
                }

                // find the new potential target group
                var targetGroup = this.groups.FirstOrDefault(g => g.Title == groupId);
                if (targetGroup == null)
                {
                    targetGroup = new Group<T>(groupId, this.GetGroupOwner(item)) { item };

                    this.InsertGroupWhereAppropriate(targetGroup);

                    this.count++;
                }
                else
                {
                    if (!targetGroup.Contains(item))
                    {
                        this.InsertWhereAppropriate(targetGroup, item);

                        this.count++;
                    }
                    else
                    {
                        this.InsertWhereAppropriate(targetGroup, item);
                    }
                }
            }

            this.OnContentPropertiesChanged();
        }

        private bool ItemMatchFilter(T item)
        {
            return this.filter == null || (this.filter != null && this.filter(item));
        }

        private void EnsureGroupIsNeeded(Group<T> group)
        {
            if (group.Count == 0)
                this.groups.Remove(group);

            this.OnContentPropertiesChanged();
        }
        
        private void OnContentPropertiesChanged()
        {
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("IsEmpty");
            this.OnPropertyChanged("IsEmptyWithHiddenItems");
            this.OnPropertyChanged("IsCompletelyEmpty");
            this.OnPropertyChanged("HiddenCount");
            this.OnPropertyChanged("HighlightsCount");
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void InsertWhereAppropriate(Group<T> source, T item)
        {
            // add it to the group
            var copy = source.ToList();
            copy.Add(item);
            copy.Sort(this.groupBuilder.ItemComparer);
            int newIndex = copy.IndexOf(item);

            int currentIndex = source.IndexOf(item);

            if (newIndex == currentIndex)
                return;

            if (currentIndex >= 0)
                source.Remove(item);

            source.Insert(newIndex, item);
        }

        private void InsertGroupWhereAppropriate(Group<T> group)
        {
            var copy = this.groups.ToList();
            copy.Add(group);
            copy.Sort(new GenericComparer<Group<T>>((a, b) => this.groupBuilder.Ascending ? this.groupBuilder.GroupComparer.Compare(a, b) : this.groupBuilder.GroupComparer.Compare(b, a)));
            int newIndex = copy.IndexOf(group);

            int currentIndex = -1;
            var existingGroup = this.groups.FirstOrDefault(g => g.Title.Equals(group.Title, StringComparison.OrdinalIgnoreCase));
            if (existingGroup != null)
                currentIndex = this.groups.IndexOf(existingGroup);

            if (newIndex == currentIndex && currentIndex >= 0)
                return;

            if (currentIndex >= 0)
                this.groups.Remove(group);

            this.groups.Insert(newIndex, group);
        }
    }
}