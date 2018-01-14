using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange.Resources;

namespace Chartreuse.Today.Exchange.Providers
{
    public class OutlookActiveSyncSynchronizationProvider : ActiveSyncSynchronizationProviderBase
    {
        public override SynchronizationService Service
        {
            get { return SynchronizationService.OutlookActiveSync; }
        }

        public override string Name
        {
            get { return ExchangeResources.OutlookActiveSync_ProviderName; }
        }

        public override string Headline
        {
            get { return ExchangeResources.OutlookActiveSync_Headline; }
        }

        public override string Description
        {
            get { return ExchangeResources.OutlookActiveSync_Description; }
        }

        public override string DefaultFolderName
        {
            get { return "Microsoft Account"; }
        }

        public OutlookActiveSyncSynchronizationProvider(ISynchronizationManager synchronizationManager, ICryptoService crypto, string deviceId) 
            : base(synchronizationManager, crypto, deviceId)
        {
        }
    }
}