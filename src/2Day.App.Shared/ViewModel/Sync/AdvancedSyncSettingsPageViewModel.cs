using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public class AdvancedSyncSettingsPageViewModel : PageViewModelBase
    {
        private readonly ISynchronizationManager synchronizationManager;
        private readonly IPlatformService platformService;
        private readonly IMessageBoxService messageBoxService;
        private readonly INavigationService navigationService;
        private readonly INotificationService notificationService;

        private readonly ObservableCollection<AdvancedSyncModeItem> items;
        
        public ObservableCollection<AdvancedSyncModeItem> Items
        {
            get { return this.items; }
        }

        protected ISynchronizationManager SynchronizationManager
        {
            get { return this.synchronizationManager; }
        }
        
        public AdvancedSyncSettingsPageViewModel(IWorkbook workbook, ISynchronizationManager synchronizationManager, IPlatformService platformService, IMessageBoxService messageBoxService, INavigationService navigationService, INotificationService notificationService)
            : base(workbook, navigationService)
        {
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (notificationService == null)
                throw new ArgumentNullException(nameof(notificationService));

            this.synchronizationManager = synchronizationManager;
            this.platformService = platformService;
            this.messageBoxService = messageBoxService;
            this.navigationService = navigationService;
            this.notificationService = notificationService;
            this.items = new ObservableCollection<AdvancedSyncModeItem>
            {
                new AdvancedSyncModeItem(
                    StringResources.SyncAdvanced_ReplaceTitle,
                    StringResources.SyncAdvanced_ReplaceDescription,
                    ResourcesLocator.SyncAdvancedModeReplace,
                    () => this.ReplaceAsync()),
                new AdvancedSyncModeItem(
                    StringResources.About_Help,
                    StringResources.Sync_FixProblems,
                    ResourcesLocator.SyncAdvancedModeDiag,
                    () => this.OpenSyncHelp()),
                new AdvancedSyncModeItem(
                    StringResources.SyncAdvanced_DiagnosticsTitle,
                    StringResources.SyncAdvanced_DiagnosticsDescription,
                    ResourcesLocator.SyncAdvancedModeDiag,
                    () => this.Diag()),
            };

            if (this.SynchronizationManager.ActiveProvider != null && this.SynchronizationManager.ActiveProvider.CanDeleteAccount)
            {
                this.items.Add(new AdvancedSyncModeItem(
                    StringResources.SyncAdvanced_DeleteAccountTitle,
                    StringResources.SyncAdvanced_DeleteAccountDescription,
                    ResourcesLocator.SyncAdvancedModeDelete,
                    () => this.Delete()));
            }
        }

        private async void Delete()
        {
            var dialogResult = await this.messageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Sync_DeleteAccount_Message, DialogButton.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string result = await this.SynchronizationManager.ActiveProvider.DeleteAccountAync();
                if (string.IsNullOrEmpty(result))
                {
                    this.SynchronizationManager.Reset(false);
                    await this.SynchronizationManager.SaveAsync();

                    var result1 = await this.messageBoxService.ShowAsync(
                        StringResources.Dialog_TitleConfirmation,
                        StringResources.Message_DeleteAllMessage,
                        DialogButton.YesNo);

                    if (result1 == DialogResult.Yes)
                        this.Workbook.RemoveAll();

                    await this.messageBoxService.ShowAsync(StringResources.Message_Information, StringResources.Sync_DeleteAccount_Success);
                }
                else
                {
                    await this.messageBoxService.ShowAsync(StringResources.Message_Warning, string.Format(StringResources.Sync_DeleteAccount_ErrorFormat, result));
                }
            }

            this.Close();
        }

        private void OpenSyncHelp()
        {
            this.platformService.OpenWebUri(Constants.HelpPageFixSyncIssues);
        }

        private async void Diag()
        {
            var builder = new StringBuilder();

            builder.AppendLine("IMPORTANT: Please give details about the problem");
            builder.AppendLine();
            builder.AppendLine();


            builder.AppendLine("The following information can help 2Day support team");
            builder.AppendLine();

            builder.Append(this.platformService.BuildDiagnosticsInformation());
            
            var notSyncedTasks = this.Workbook.Tasks.Where(t => string.IsNullOrEmpty(t.SyncId)).Select(t => string.Format("{0} ({1})", t.Title, t.Folder.Name));
            if (notSyncedTasks.Any())
                builder.AppendLine($"Missing sync id {notSyncedTasks.AggregateString()}");
            else
                builder.AppendLine("All tasks have a sync id");

            await this.messageBoxService.ShowAsync(StringResources.Message_Information, StringResources.SyncAdvanced_EmailDetail);

            await this.platformService.SendDiagnosticEmailAsync("2Day Sync Diagnostics", builder.ToString());

            this.Close();
        }

        private async Task ReplaceAsync()
        {
            this.SynchronizationManager.OperationStarted += this.OnSyncStarted;
            this.SynchronizationManager.OperationProgressChanged += this.OnSyncProgressChanged;
            this.SynchronizationManager.OperationCompleted += this.OnSyncOperationCompleted;
            this.SynchronizationManager.OperationFailed += this.OnSyncOperationFailed;

            try
            {
                await this.SynchronizationManager.AdvancedSync(SyncAdvancedMode.Replace);
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Replace sync service: " + this.SynchronizationManager.ActiveService);
                this.messageBoxService.ShowAsync(StringResources.Message_Warning, ex.ToString());
            }

            this.SynchronizationManager.OperationStarted -= this.OnSyncStarted;
            this.SynchronizationManager.OperationProgressChanged -= this.OnSyncProgressChanged;
            this.SynchronizationManager.OperationCompleted -= this.OnSyncOperationCompleted;
            this.SynchronizationManager.OperationFailed -= this.OnSyncOperationFailed;

            this.Close();
        }

        private void Close()
        {
            this.navigationService.CloseFlyouts();
        }

        private void OnSyncStarted(object sender, EventArgs e)
        {
            this.notificationService.StartAsyncOperation();
        }

        private async void OnSyncOperationFailed(object sender, SyncFailedEventArgs e)
        {
            this.notificationService.EndAsyncOperationAsync();
        }

        private async void OnSyncOperationCompleted(object sender, EventArgs<string> e)
        {
            this.notificationService.EndAsyncOperationAsync(StringResources.HomePage_SyncCompletedSystrayMessage);
        }

        private void OnSyncProgressChanged(object sender, EventArgs<string> e)
        {
            this.notificationService.ReportProgressAsyncOperation(e.Item);
        }
    }
}
