using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public abstract class SyncSettingsViewModelBase : PageViewModelBase
    {
        private readonly IMessageBoxService messageBoxService;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly ITrackingManager trackingManager;
        private readonly ICommand checkSettingsCommand;

        protected ISynchronizationManager SynchronizationManager
        {
            get { return this.synchronizationManager; }
        }
        
        public ICommand CheckSettingsCommand
        {
            get { return this.checkSettingsCommand; }
        }

        protected IMessageBoxService MessageBoxService
        {
            get { return this.messageBoxService; }
        }

        protected ITrackingManager TrackingManager
        {
            get { return this.trackingManager; }
        }

        protected abstract SynchronizationService SynchronizationService
        {
            get;
        }

        protected SyncSettingsViewModelBase(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager) : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.messageBoxService = messageBoxService;
            this.synchronizationManager = synchronizationManager;
            this.trackingManager = trackingManager;
            this.checkSettingsCommand = new RelayCommand(this.CheckSettingsExecute);

            this.trackingManager.TagEvent("Sync settings open", new Dictionary<string, string> { { "service", this.SynchronizationService.ToString() } });
        }

        protected abstract void CheckSettingsExecute();

        protected void HandleBadSettings()
        {
            this.TrackingManager.TagEvent("Sync settings failed", new Dictionary<string, string> { { "service", this.SynchronizationService.ToString() } });
        }

        protected async Task HandleGoodSettings()
        {
            this.trackingManager.TagEvent("Sync settings success", new Dictionary<string, string> { { "service", this.SynchronizationService.ToString() } });

            this.NavigationService.GoBack();
            await this.synchronizationManager.SaveAsync();
        }
    }
}
