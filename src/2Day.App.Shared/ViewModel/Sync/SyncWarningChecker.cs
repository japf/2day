using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public static class SyncWarningChecker
    {
        public static async Task CheckWarningBeforeSync(IWorkbook workbook, ISynchronizationManager syncManager, IMessageBoxService messageBoxService, IPlatformService platformService)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            if (syncManager == null)
                throw new ArgumentNullException("syncManager");
            if (messageBoxService == null)
                throw new ArgumentNullException("messageBoxService");
            if (platformService == null)
                throw new ArgumentNullException("platformService");
            
            // check if context warning has already been shown
            if (!workbook.Settings.GetValue<bool>(CoreSettings.SyncWarningContextNotSupported))
            {
                if (workbook.Contexts.Count > 0 && !syncManager.ActiveProviderSupportFeature(SyncFeatures.Context))
                {
                    // show warning
                    var result = await messageBoxService.ShowAsync(
                        StringResources.Dialog_TitleConfirmation, 
                        StringResources.Dialog_SyncContextsNotSupported, 
                        DialogButton.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        await platformService.OpenWebUri(Constants.HelpPageChooseSyncAddress);
                    }

                    workbook.Settings.SetValue(CoreSettings.SyncWarningContextNotSupported, true);
                }
            }
        }

        public static async Task HandleFailedSynced(SyncFailedEventArgs e, IMessageBoxService messageBoxService, INavigationService navigationService)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            string message = e.Message ?? string.Empty;
            string details = null;

            if (e.Exception != null)
                details = ExceptionExtension.BeautifyAsyncStacktrace(e.Exception.ToString());

            if (!string.IsNullOrEmpty(message))
            {
                if (!string.IsNullOrEmpty(details))
                {
                    var result = await messageBoxService.ShowAsync(
                        StringResources.Sync_FailedMessage, 
                        message, 
                        new[] { StringResources.General_LabelOk, StringResources.General_LabelDetails });

                    if (result.ButtonIndex == 1)
                    {
                        details = string.Format("{0}\r\n{1}", message, details);
                        await messageBoxService.ShowAsync(StringResources.Message_Information, details);
                    }
                }
                else
                {
                    await messageBoxService.ShowAsync(StringResources.Message_Warning, message);
                }

                if (message.Equals(StringResources.Exchange_InvalidSyncKeyWorkaround))
                {
                    navigationService.FlyoutTo(ViewLocator.SyncAdvancedSyncSettingsPage);
                }
            }
        }
    }
}
