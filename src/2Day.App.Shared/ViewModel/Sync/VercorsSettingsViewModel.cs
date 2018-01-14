using System;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Vercors.Shared.Resources;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public class VercorsSettingsViewModel : SyncSettingsViewModelBase
    {
        protected override SynchronizationService SynchronizationService
        {
            get { return SynchronizationService.Vercors; }
        }

        public VercorsSettingsViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager) 
            : base(workbook, navigationService, messageBoxService, synchronizationManager, trackingManager)
        {
        }

        protected override async void CheckSettingsExecute()
        {
            using (this.ExecuteBusyAction(StringResources.SyncProgress_SignInIn))
            {
                var provider = this.SynchronizationManager.GetProvider(SynchronizationService.Vercors);

                try
                {
                    provider.OperationFailed += this.OnProviderOperationFailed;
                    var result = await provider.CheckLoginAsync();
                    provider.OperationFailed -= this.OnProviderOperationFailed;

                    if (result)
                    {
                        this.SynchronizationManager.Metadata.Reset();
                        this.SynchronizationManager.ActiveService = SynchronizationService.Vercors;

                        await this.HandleGoodSettings();
                    }
                    else
                    {
                        await this.MessageBoxService.ShowAsync(StringResources.General_LabelError, VercorsResources.Vercors_UnableToLogin);                        
                    }
                }
                catch (Exception ex)
                {
                    await this.MessageBoxService.ShowAsync(StringResources.General_LabelError, ex.Message);
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
