using System;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Services
{
    public interface IPlatformService
    {
        string Version { get; }
        AppPlatform Platform { get; }
        DeviceFamily DeviceFamily { get; }
        string DeviceId { get; }
        string WindowsVersion { get; }

        bool HasHardwareBackButton { get; }
        bool IsNetworkAvailable { get; }

        bool IsDebug { get; }

        Task SendDiagnosticEmailAsync(string subject, string content);
        Task OpenWebUri(string uri);

        void ClearSearchHistory();

        Task DeleteFileAsync(string path);
        Task SaveFileAsync<T>(T item, string filename);
        Task<T> LoadFileAsync<T>(string filename);

        Task ExitAppAsync();
        void DispatchActionIdleAsync(Action action);

        Task<string> PickImageAsync(string source);

        void ShowPhoneCallUI(string phoneNumber, string displayName);

        T GetSettingValue<T>(string settingKey);

        string BuildDiagnosticsInformation();
    }
}
