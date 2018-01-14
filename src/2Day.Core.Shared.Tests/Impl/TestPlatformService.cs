using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.Core.Shared.Tests.Impl
{
    public class TestPlatformService : IPlatformService
    {
        private readonly Dictionary<string, object> savedItems = new Dictionary<string, object>();
        
        public string Version
        {
            get { return string.Empty; }
        }

        public string Build
        {
            get { return string.Empty; }
        }

        public AppPlatform Platform
        {
            get { return AppPlatform.Unkown; }
        }

        public DeviceFamily DeviceFamily
        {
            get { return DeviceFamily.Unkown; }
        }

        public string DeviceId
        {
            get { return string.Empty; }
        }

        public string WindowsVersion
        {
            get { return string.Empty; }
        }

        public bool HasHardwareBackButton
        {
            get { return false; }
        }

        public bool IsNetworkAvailable
        {
            get { return true; }
        }
        
        public bool IsDebug
        {
            get { return false; }
        }
        
        public Task SendDiagnosticEmailAsync(string subject, string content)
        {
            return null;
        }

        public Task OpenWebUri(string uri)
        {
            return null;
        }

        public void ClearSearchHistory()
        {
        }

        public Task DeleteFileAsync(string path)
        {
            return null;
        }

        public Task SaveFileAsync<T>(T item, string filename)
        {
            if (this.savedItems.ContainsKey(filename))
                this.savedItems[filename] = item;
            else
                this.savedItems.Add(filename, item);

            return Task.FromResult(0);
        }

        public Task<T> LoadFileAsync<T>(string filename)
        {
            if (this.savedItems.ContainsKey(filename))
                return Task.FromResult((T)this.savedItems[filename]);

            return null;
        }

        public Task ExitAppAsync()
        {
            return Task.FromResult(0);
        }

        public Task<string> PickImageAsync(string source)
        {
            return null;
        }

        public void ShowPhoneCallUI(string phoneNumber, string displayName)
        {
        }

        public T GetSettingValue<T>(string settingKey)
        {
            return default(T);
        }

        public string BuildDiagnosticsInformation()
        {
            return null;
        }

        public void DispatchActionIdleAsync(Action action)
        {
        }
    }
}