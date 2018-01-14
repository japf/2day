using System;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange.Resources;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class SyncProviderViewModel
    {
        private readonly ICommand configureCommand;
        private readonly ICommand removeCommand;
        private readonly bool isRecommended;

        private readonly string loginInfoPrimary;
        private readonly string loginInfoSecondary;

        private readonly ISynchronizationProvider provider;
        private readonly ISynchronizationMetadata synchronizationMetadata;

        public string LoginInfoPrimary
        {
            get { return this.loginInfoPrimary; }
        }

        public string LoginInfoSecondary
        {
            get { return this.loginInfoSecondary; }
        }

        public string FolderInfo
        {
            get { return this.provider != null ? this.provider.FolderInfo : string.Empty; }
        }

        public bool IsRecommended
        {
            get { return this.isRecommended; }
        }

        public virtual string Headline
        {
            get { return this.provider.Headline; }
        }
        
        public virtual string Description
        {
            get { return this.provider.Description; }
        }

        public virtual string Icon
        {
            get { return ResourcesLocator.GetSyncServiceIcon(this.provider.Service, this.provider.ServerInfo); }
        }

        public ICommand ConfigureCommand
        {
            get { return this.configureCommand; }
        }

        public ICommand RemoveCommand
        {
            get { return this.removeCommand; }
        }

        public SynchronizationService Service
        {
            get { return this.provider.Service; }
        }

        public string LastSync
        {
            get
            {
                if (this.synchronizationMetadata.LastSync != DateTime.MinValue)
                    return this.synchronizationMetadata.LastSync.ToString("g");
                else
                    return StringResources.SyncSettingsPage_LastSyncNever;
            }
        }

        public SyncProviderViewModel(ISynchronizationProvider activeProvider, ISynchronizationManager synchronizationManager, bool isRecommended, Action configure = null, Action remove = null)
        {
            if (activeProvider == null)
                throw new ArgumentNullException("activeProvider");

            this.provider = activeProvider;
            this.isRecommended = isRecommended;
            this.synchronizationMetadata = synchronizationManager.Metadata;

            if (configure == null)
                configure = () => { };
            if (remove == null)
                remove = () => { };

            this.configureCommand = new RelayCommand(configure);
            this.removeCommand = new RelayCommand(remove);

            if (this.provider != null && !string.IsNullOrWhiteSpace(this.provider.LoginInfo))
            {
                if (this.provider.LoginInfo.Contains("@"))
                {
                    var split = this.provider.LoginInfo.Split('@');
                    this.loginInfoPrimary = split[0];
                    if (split.Length > 1)
                        this.loginInfoSecondary = "@" + split[1];
                }
                else
                {
                    this.loginInfoPrimary = this.provider.LoginInfo;
                }
            }
            else
            {
                this.loginInfoPrimary = string.Empty;
                this.loginInfoSecondary = string.Empty;
            }
        } 
    }

    public class Office365SyncProviderViewModel : SyncProviderViewModel
    {
        public override string Headline
        {
            get { return ExchangeResources.ExchangeOffice365_Headline; }
        }

        public override string Description
        {
            get { return ExchangeResources.ExchangeOffice365_Description; }
        }

        public override string Icon
        {
            get { return ResourcesLocator.syncOffice365IconUri.ToString(); }
        }

        public Office365SyncProviderViewModel(ISynchronizationProvider activeProvider, ISynchronizationManager synchronizationManager, bool isRecommended, Action configure = null, Action remove = null)
            : base(activeProvider, synchronizationManager, isRecommended, configure, remove)
        {
        }
    }

    public class ExchangeSyncProviderViewModel : SyncProviderViewModel
    {
        public override string Icon
        {
            get { return ResourcesLocator.syncExchangeIconUri.ToString(); }
        }

        public ExchangeSyncProviderViewModel(ISynchronizationProvider activeProvider, ISynchronizationManager synchronizationManager, bool isRecommended, Action configure = null, Action remove = null)
            : base(activeProvider, synchronizationManager, isRecommended, configure, remove)
        {
        }
    }
}
