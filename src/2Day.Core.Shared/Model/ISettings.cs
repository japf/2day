using System;

namespace Chartreuse.Today.Core.Shared.Model
{
    public interface ISettings
    {
        event EventHandler<SettingsKeyChanged> KeyChanged;

        bool HasValue(string key);
        T GetValue<T>(string key);
        void SetValue<T>(string key, T value);
    }
}
