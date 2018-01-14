using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class QuickAddTaskViewModel : CreateTaskViewModel
    {        
        public event EventHandler Saved;

        public QuickAddTaskViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, INotificationService notificationService, ISynchronizationManager synchronizationManager, ISpeechService speechService, ITrackingManager trackingManager, IPlatformService platformService)
            : base(workbook, navigationService, messageBoxService, notificationService, synchronizationManager, speechService, trackingManager, platformService)
        {
            this.NotifyAdd = true;
        }

        protected override async Task<ITask> SaveExecuteCore(bool navigateBack)
        {
            ITask task = await base.SaveExecuteCore(navigateBack);

            if (this.TargetFolder != null)
                this.Settings.SetValue(CoreSettings.LastQuickAddFolderId, this.TargetFolder.Id);

            this.Saved?.Invoke(this, EventArgs.Empty);

            return task;
        }
    }
}