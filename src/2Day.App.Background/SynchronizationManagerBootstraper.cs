using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.ToodleDo;
using Chartreuse.Today.Vercors.Shared;

namespace Chartreuse.Today.App.Background
{
    public static class SynchronizationManagerBootstraper
    {
        public static async Task InitializeAsync(IWorkbook workbook, ISynchronizationManager synchronizationManager, ICryptoService cryptoService, IVercorsService vercorsService, bool awaitPrepareProvider)
        {
            if (synchronizationManager == null)
                throw new ArgumentNullException("synchronizationManager");
            if (cryptoService == null)
                throw new ArgumentNullException("cryptoService");
            if (vercorsService == null)
                throw new ArgumentNullException("vercorsService");

            await synchronizationManager.InitializeAsync();

            synchronizationManager.RegisterProvider(
                SynchronizationService.ToodleDo, 
                () => new ToodleDoSynchronizationProvider(synchronizationManager, cryptoService));

            synchronizationManager.RegisterProvider(
                SynchronizationService.Exchange, 
                () => new ExchangeSynchronizationProvider(synchronizationManager, cryptoService));

            synchronizationManager.RegisterProvider(
                SynchronizationService.ExchangeEws, 
                () => new ExchangeEwsSynchronizationProvider(synchronizationManager, cryptoService));

            synchronizationManager.RegisterProvider(
                SynchronizationService.OutlookActiveSync, 
                () => new OutlookActiveSyncSynchronizationProvider(synchronizationManager, cryptoService, workbook.Settings.GetValue<string>(CoreSettings.DeviceId)));

            synchronizationManager.RegisterProvider(
                SynchronizationService.Vercors, 
                () => new VercorsSynchronizationProvider(synchronizationManager, cryptoService, vercorsService));

            synchronizationManager.RegisterProvider(
                SynchronizationService.ActiveSync,
                () => new ActiveSyncSynchronizationProvider(synchronizationManager, cryptoService, workbook.Settings.GetValue<string>(CoreSettings.DeviceId)));

            synchronizationManager.AttachWorkbook(workbook);

            if (awaitPrepareProvider)
                await synchronizationManager.PrepareProviderAsync();
            else
                #pragma warning disable 4014
                synchronizationManager.PrepareProviderAsync();
                #pragma warning restore 4014
        }
    }
}
