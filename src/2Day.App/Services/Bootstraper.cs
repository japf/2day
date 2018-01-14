using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Manager;
using Chartreuse.Today.App.Manager.UI;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.Net;
using Chartreuse.Today.App.VoiceCommand;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.Core.Universal.Tools;
using Chartreuse.Today.Core.Universal.Tools.Security;
using Chartreuse.Today.Shared.Sync.Vercors;
using Task = System.Threading.Tasks.Task;
using Chartreuse.Today.App.Background;

namespace Chartreuse.Today.App.Services
{
    public class Bootstraper : BootstraperBase<Frame>
    {
        private KeyboardShortcutManager keyboardShortcutManager;

        public Bootstraper(string version) : base(version)
        {
        }

        protected override IWorkbook CreateWorkbook(IDatabaseContext context)
        {
            return new Workbook(context, WinSettings.Instance);
        }

        public override async Task<IWorkbook> ConfigureAsync(Frame rootFrame)
        {
            if (rootFrame == null)
                throw new ArgumentNullException(nameof(rootFrame));

            bool sendAnalytics = WinSettings.Instance.GetValue<bool>(CoreSettings.SendAnalytics);
            
            string deviceId = "n/a";
            try
            {
                var settings = WinSettings.Instance;
                if (!settings.HasValue(CoreSettings.DeviceId) || settings.GetValue<string>(CoreSettings.DeviceId) == "N/A")
                {
                    string id = this.ComputeDeviceId();
                    settings.SetValue(CoreSettings.DeviceId, id);
                }

                deviceId = settings.GetValue<string>(CoreSettings.DeviceId);
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Error while setting device id");
            }

            var platformService = new PlatformService(this.version, deviceId, () => CrashHandler.GetDiagnosticsInformation());
            Ioc.RegisterInstance<IPlatformService, PlatformService>(platformService);

            TrackingManager trackingManager = new TrackingManager(sendAnalytics, platformService.DeviceFamily);
            Ioc.RegisterInstance<ITrackingManager, TrackingManager>(trackingManager);

            HttpClientHandler.Setup();
            
            ResourcesLocator.Initialize("ms-appx://", UriKind.Absolute);
            if (!WinSettings.Instance.GetValue<bool>(CoreSettings.UseDarkTheme))
                ResourcesLocator.UpdateTheme(true);

            var persistence = Ioc.RegisterInstance<IPersistenceLayer, WinPersistenceLayer>(new WinPersistenceLayer(automaticSave: true));

            LogService.Log("Bootstraper", string.Format("Version: {0}", ApplicationVersion.GetAppVersion()));

            var navigationService = new NavigationService(rootFrame, platformService);
            Ioc.RegisterInstance<INavigationService, NavigationService>(navigationService);

            var messageBoxService = new MessageBoxService(navigationService);
            Ioc.RegisterInstance<IMessageBoxService, MessageBoxService>(messageBoxService);

            Ioc.RegisterInstance<ISpeechService, SpeechService>(new SpeechService(messageBoxService));

            var workbook = this.InitializeWorkbook(persistence, platformService);

            // we just read the latest value of data from the DB, so even if there was a background sync
            // we now have the latest version, so we remove the background sync flag here
            workbook.Settings.SetValue(CoreSettings.SyncBackgroundOccured, false);

            this.keyboardShortcutManager = new KeyboardShortcutManager(workbook, rootFrame, navigationService, trackingManager);

            var backgroundTaskManager = new BackgroundTaskManager(workbook);
            backgroundTaskManager.InitializeAsync();
            Ioc.RegisterInstance<IBackgroundTaskManager, BackgroundTaskManager>(backgroundTaskManager);

            var notificationService = new NotificationService();
            Ioc.RegisterInstance<INotificationService, NotificationService>(notificationService);

            var startupManager = Ioc.Build<StartupManager>();
            Ioc.RegisterInstance<IStartupManager, StartupManager>(startupManager);
            
            Ioc.RegisterInstance<IAlarmManager, AlarmManager>(new AlarmManager(workbook));

            var tileManager = new TileManager(workbook, trackingManager, notificationService, false);
            tileManager.LoadSecondaryTilesAsync();
            Ioc.RegisterInstance<ITileManager, TileManager>(tileManager);

            var synchronizationManager = await InitializeSyncAsync(workbook, platformService, trackingManager);
            
            // it is important to remove old task after sync is initialized otherwise changes would not
            // tracked and put in the changelog for the next sync
            workbook.RemoveOldTasks();
            
            var cortanaService = new CortanaRuntimeService(workbook);
            // no need to await this call because it can runs in the background
            Task.Run(() =>
            {
                JumpListManager.SetupJumpListAsync();
                cortanaService.SetupDefinitionsAsync();
            });
            Ioc.RegisterInstance<ICortanaRuntimeService, CortanaRuntimeService>(cortanaService);

            return workbook;
        }
        
        private static async Task<SynchronizationManager> InitializeSyncAsync(IWorkbook workbook, IPlatformService platformService, ITrackingManager trackingManager)
        {
            var cryptoService = Ioc.RegisterInstance<ICryptoService, WinCryptoService>(new WinCryptoService());
            
            var synchronizationManager = new SynchronizationManager(platformService, trackingManager, "win", false);

            await SynchronizationManagerBootstraper.InitializeAsync(workbook, synchronizationManager, cryptoService, new VercorsService(), false);
            
            Ioc.RegisterInstance<ISynchronizationManager, SynchronizationManager>(synchronizationManager);

            return synchronizationManager;
        }

        private string ComputeDeviceId()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            {
                HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;
                var dataReader = DataReader.FromBuffer(hardwareId);

                byte[] bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);

                return BitConverter.ToString(bytes).Replace("-", string.Empty);
            }
            else
            {
                TrackingManagerHelper.Trace("Could not find HardwareIdentification to have device id");
            }

            return "NA";
        }
    }
}
