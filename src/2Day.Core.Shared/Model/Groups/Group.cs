using System.ComponentModel;
using System.Diagnostics;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.Core.Shared.Model.Groups
{
    [DebuggerDisplay("Title: {Title} Count: {Count}")]
    public class Group<T> : SortableObservableCollection<T>
    {
        private readonly object owner;
        private string title;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Title"));                    
                }
            }
        }

        public bool HasItems
        {
            get
            {
                return this.Count != 0;
            }
        }

        public object Owner
        {
            get { return this.owner; }
        }

        public Group(string name, object owner)
        {
            this.owner = owner;
            this.Title = name;
        }

        public override string ToString()
        {
            return string.Format("{0} Items: {1}", this.Title, this.Count);
        }
    }
}
