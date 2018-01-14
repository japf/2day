using System.ComponentModel;

namespace Chartreuse.Today.Core.Shared.Model
{
    public class ValueChangedEventArgs : PropertyChangedEventArgs
    {
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }

        public ValueChangedEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}