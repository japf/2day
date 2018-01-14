using System.ComponentModel;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName = null)
        {
            if (propertyName == null)
                propertyName = string.Empty;

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
