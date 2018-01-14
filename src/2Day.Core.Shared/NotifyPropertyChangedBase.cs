using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chartreuse.Today.Core.Shared
{
    /// <summary>
    /// Provides a base implementation of <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event for multiple properties.
        /// </summary>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (string name in propertyNames)
                OnPropertyChanged(name);
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for multiple properties.
        /// </summary>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        protected virtual void OnPropertyChanged(IEnumerable<string> propertyNames)
        {
            foreach (string name in propertyNames)
                OnPropertyChanged(name);
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for a single property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets a notification delegate, which can be used to delay notification.
        /// </summary>
        /// <param name="names">The names of the updated properties.</param>
        /// <returns>A notification delegate.</returns>
        protected Action PendingUpdates(params string[] names)
        {
            if ((names == null) || (names.Length == 0))
                return null;
            return () => this.OnPropertyChanged(names);
        }
    }
}
