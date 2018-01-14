using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public class PlatformService : IPlatformService
    {
        private readonly string version;

        private readonly string deviceId;
        private readonly Func<string> getDiagnosticsInformation;
        private readonly DeviceFamily deviceFamily;
        private string deviceVersion;

        public string Version
        {
            get { return this.version; }
        }
        
        public AppPlatform Platform
        {
            get { return AppPlatform.WindowsUniversal; }
        }

        public DeviceFamily DeviceFamily
        {
            get { return this.deviceFamily; }
        }

        public string DeviceId
        {
            get {  return this.deviceId; }
        }

        public string WindowsVersion
        {
            get
            {
                if (this.deviceVersion == null)
                {
                    try
                    {
                        // get the system version number
                        string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                        ulong v = ulong.Parse(sv);
                        ulong v1 = (v & 0xFFFF000000000000L) >> 48;
                        ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
                        ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
                        ulong v4 = (v & 0x000000000000FFFFL);

                        // get the package architecure
                        Package package = Package.Current;

                        this.deviceVersion = $"{v1}.{v2}.{v3}.{v4} {package.Id.Architecture}";
                    }
                    catch (Exception ex)
                    {
                        TrackingManagerHelper.Exception(ex, "While building device version");
                        this.deviceVersion = "unknown";
                    }
                }

                return this.deviceVersion;
            }
        }

        public PlatformService(string version, string deviceId, Func<string> getDiagnosticsInformation)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));
            if (getDiagnosticsInformation == null)
                throw new ArgumentNullException(nameof(getDiagnosticsInformation));

            this.version = version;
            this.deviceId = deviceId;
            this.getDiagnosticsInformation = getDiagnosticsInformation;

            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop", StringComparison.OrdinalIgnoreCase))
                this.deviceFamily = DeviceFamily.WindowsDesktop;
            else if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.OrdinalIgnoreCase))
                this.deviceFamily = DeviceFamily.WindowsMobile;
            else
                this.deviceFamily = DeviceFamily.Unkown;            
        }

        public bool HasHardwareBackButton
        {
            get { return Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"); }
        }

        public bool IsNetworkAvailable
        {
            get { return NetworkHelper.IsNetworkAvailable; }
        }
        
        public bool IsDebug
        {
            get { return WinLicenseManager.IsDebug; }
        }
        
        public void ClearSearchHistory()
        {
            throw new NotSupportedException();
        }

        public async Task DeleteFileAsync(string path)
        {
            await WinIsolatedStorage.DeleteAsync(path);
        }

        public async Task<T> LoadFileAsync<T>(string filename)
        {
            return await WinIsolatedStorage.RestoreAsync<T>(filename);
        }

        public async Task ExitAppAsync()
        {
            if (Ioc.HasType<IPersistenceLayer>())
                Ioc.Resolve<IPersistenceLayer>().Save();

            if (Ioc.HasType<ISynchronizationManager>())
                await Ioc.Resolve<ISynchronizationManager>().SaveAsync();

            await LogService.SaveAsync();

            Application.Current.Exit();
        }

        public void DispatchActionIdleAsync(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Window.Current.Dispatcher.RunIdleAsync((e) => action());
        }

        public async Task<string> PickImageAsync(string source)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile result = await picker.PickSingleFileAsync();
            if (result != null && !string.IsNullOrEmpty(result.Path))
            {
                if (!string.IsNullOrEmpty(source))
                    WinIsolatedStorage.DeleteAsync(source);

                string filename = "background-" + Guid.NewGuid() + Path.GetExtension(result.Path);
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                await result.CopyAndReplaceAsync(file);

                return filename;
            }

            return null;
        }

        public void ShowPhoneCallUI(string phoneNumber, string displayName)
        {
            if (phoneNumber == null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (displayName == null)
                throw new ArgumentNullException(nameof(displayName));

            PhoneCallManager.ShowPhoneCallUI(phoneNumber, displayName);
        }

        public T GetSettingValue<T>(string settingKey)
        {
            return WinSettings.Instance.GetValue<T>(settingKey);
        }

        public string BuildDiagnosticsInformation()
        {
            return this.getDiagnosticsInformation();
        }

        public async Task SaveFileAsync<T>(T item, string filename)
        {
            await WinIsolatedStorage.SaveAsync(item, filename);            
        }

        public async Task SendDiagnosticEmailAsync(string subject, string content)
        {
            var email = new EmailMessage
            {
                Subject = subject,
                Body = content
            };
            email.To.Add(new EmailRecipient(Constants.SupportEmailAddress));

            await MailHelper.AddAttachment(email, LogService.Filename, "logs.txt");

            await EmailManager.ShowComposeNewEmailAsync(email);            
        }

        public async Task OpenWebUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");

            try
            {
                if (!uri.StartsWith("http", StringComparison.OrdinalIgnoreCase) 
                    && !uri.StartsWith("https", StringComparison.OrdinalIgnoreCase) 
                    && !uri.StartsWith("ms-windows-store", StringComparison.OrdinalIgnoreCase)
                    && !uri.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                    uri = "http://" + uri;

                await Launcher.LaunchUriAsync(SafeUri.Get(uri, UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, $"Unable to open web uri: {uri}");
            }
        }                
    }
}
