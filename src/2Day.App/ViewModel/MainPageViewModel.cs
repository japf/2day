using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Chartreuse.Today.App.Background;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.ViewModel
{
    public class MainPageViewModel : MainPageViewModelBase
    {
        private readonly PrintHelper printHelper;
        private readonly AppJobScheduler jobScheduler;
        
        public MainPageViewModel(IWorkbook workbook, ISynchronizationManager synchronizationManager, IStartupManager startupManager, IMessageBoxService messageBoxService, INotificationService notificationService, INavigationService navigationService, IPlatformService platformService, ITileManager tileManager, ITrackingManager trackingManager, ISpeechService speechService) 
            : base(workbook, synchronizationManager, startupManager, messageBoxService, notificationService, navigationService, platformService, tileManager, trackingManager, speechService)
        {
            this.printHelper = new PrintHelper(this.messageBoxService);
            this.jobScheduler = new AppJobScheduler(this.Workbook, this.synchronizationManager, () => this.MenuItems.OfType<FolderItemViewModel>());
        }

        protected override async void PrintExecute()
        {
            // for testing purpose, open the page directly so that we can live inspect the visual tree
            // this.NavigationService.Navigate(typeof(PrintPage), this.SelectedFolderItem);

            if (this.SelectedFolderItem != null)
                await this.printHelper.RequestPrintAsync(this.SelectedFolderItem);
        }

        public override async Task RefreshAsync()
        {
            try
            {
                if (this.Workbook.Settings.GetValue<bool>(CoreSettings.SyncBackgroundOccured))
                {
                    var backgroundSyncManager = new BackgroundSynchronizationManager(this.Workbook, this.trackingManager, (msg) => { });
                    await backgroundSyncManager.TryUpdateWorkbookAsync();

                    this.Workbook.Settings.SetValue(CoreSettings.SyncBackgroundOccured, false);
                }

                this.jobScheduler.OnUpdateTasksTimerTick();
            }
            catch (Exception ex)
            {
                this.trackingManager.Exception(ex, $"Exception during RefreshAsync: {ex.Message}");
            }
        }

        protected override void ShareExecute()
        {
            DataTransferManager.ShowShareUI();
        }        
    }
}
