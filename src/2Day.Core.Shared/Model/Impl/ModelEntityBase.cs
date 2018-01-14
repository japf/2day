using System;
using System.ComponentModel;

namespace Chartreuse.Today.Core.Shared.Model.Impl
{
    public class ModelEntityBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null)
                throw new ArgumentNullException(nameof(propertyNames));

            for (int i = 0; i < propertyNames.Length; i++)
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyNames[i]));
        }
    }
}
