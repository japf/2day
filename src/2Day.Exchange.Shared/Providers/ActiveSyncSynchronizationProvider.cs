using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange.Resources;

namespace Chartreuse.Today.Exchange.Providers
{
    public class ActiveSyncSynchronizationProvider : ActiveSyncSynchronizationProviderBase
    {
        public override string Name
        {
            get { return ExchangeResources.ActiveSync_ProviderName; }
        }

        public override string Headline
        {
            get { return ExchangeResources.ActiveSync_Headline; }
        }

        public override string Description
        {
            get { return ExchangeResources.ActiveSync_Description; }
        }

        public override string DefaultFolderName
        {
            get { return "Default Folder"; }
        }

        public override SynchronizationService Service
        {
            get { return SynchronizationService.ActiveSync; }
        }

        public ActiveSyncSynchronizationProvider(ISynchronizationManager synchronizationManager, ICryptoService crypto, string deviceId) 
            : base(synchronizationManager, crypto, deviceId)
        {
        }
    }
}