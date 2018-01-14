using System;

namespace Chartreuse.Today.Core.Shared.Model
{
    public class SettingsKeyChanged : EventArgs
    {
        public string Key { get; private set; }
        public object Value { get; private set; }
        
        public SettingsKeyChanged(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            this.Key = key;
            this.Value = value;
        }
    }
}
