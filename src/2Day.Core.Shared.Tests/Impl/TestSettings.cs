using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Tests.Impl
{
    public class TestSettings : ISettings
    {
        private readonly Dictionary<string, object> map; 

        public event EventHandler<SettingsKeyChanged> KeyChanged;

        public static TestSettings Instance = new TestSettings();

        public TestSettings()
        {
            this.map = new Dictionary<string, object>();
        }

        public bool HasValue(string key)
        {
            return this.map.ContainsKey(key);
        }

        public T GetValue<T>(string key)
        {
            if (this.HasValue(key))
                return (T) this.map[key];
            else
                return default(T);
        }

        public void SetValue<T>(string key, T value)
        {
            this.map[key] = value;

            if (this.KeyChanged != null)
                this.KeyChanged(this, new SettingsKeyChanged(key, value));
        }
    }
}