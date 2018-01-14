using System;
using System.Text;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestPlatformService : IPlatformService
    {
        public string Version { get; }
        public string Build { get; }
        public AppPlatform Platform { get; }
        public DeviceFamily DeviceFamily { get; }
        public string DeviceId { get; }
        public string WindowsVersion { get; }
        public bool HasHardwareBackButton { get; }

        public bool IsNetworkAvailable
        {
            get { return false; }
        }

        public bool HasTrialExpired
        {
            get { return false; }
        }

        public bool IsTrial
        {
            get { return false; }
        }

        public bool IsBeta
        {
            get { return false; }
        }

        public bool IsDebug
        {
            get { return false; }
        }

        public int DaysBeforeTrialExpiration
        {
            get { return 7; }
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

        public Task<T> LoadFileAsync<T>(string filename)
        {
            return null;
        }

        public Task ExitAppAsync()
        {
            return Task.FromResult(0);
        }

        public void DispatchActionIdleAsync(Action action)
        {
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

        public Task SaveFileAsync<T>(T item, string filename)
        {
            return null;
        }
    }
}