using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.ToodleDo.Shared;
using Chartreuse.Today.ToodleDo.Shared.Resources;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public class ToodleDoSettingsViewModel : SyncSettingsViewModelBase
    {
        public string Email
        {
            get
            {
                return this.Settings.GetValue<string>(ToodleDoSettings.ToodleDoLogin);
            }
            set
            {
                if (this.Email != value)
                {
                    this.Settings.SetValue(ToodleDoSettings.ToodleDoLogin, value);
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        protected override SynchronizationService SynchronizationService
        {
            get { return SynchronizationService.ToodleDo; }
        }

        public ToodleDoSettingsViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager) 
            : base(workbook, navigationService, messageBoxService, synchronizationManager, trackingManager)
        {
        }

        protected override async void CheckSettingsExecute()
        {
            using (this.ExecuteBusyAction(ToodleDoResources.ToodleDo_CheckingCredentials))
            {
                ISynchronizationProvider provider = this.SynchronizationManager.GetProvider(SynchronizationService.ToodleDo);
                provider.OperationFailed += this.OnProviderOperationFailed;
                var result = await provider.CheckLoginAsync();
                provider.OperationFailed -= this.OnProviderOperationFailed;

                if (result)
                {
                    this.SynchronizationManager.Metadata.Reset();
                    this.SynchronizationManager.ActiveService = SynchronizationService.ToodleDo;

                    await this.HandleGoodSettings();
                }
            }
        }

        private async void OnProviderOperationFailed(object sender, EventArgs<string> e)
        {
            this.HandleBadSettings();

            await this.MessageBoxService.ShowAsync(StringResources.Message_Warning, e.Item);
        }
    }
}
