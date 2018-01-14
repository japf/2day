using System;
using System.Collections.ObjectModel;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class AboutPageViewModel : PageViewModelBase
    {
        private readonly IPlatformService platformService;
        private readonly ObservableCollection<HeadedSettingItemViewModel> items;

        public string Version
        {
            get { return this.platformService.Version; }
        }
        
        public ObservableCollection<HeadedSettingItemViewModel> Items
        {
            get { return this.items; }
        }

        public AboutPageViewModel(IWorkbook workbook, INavigationService navigationService, IPlatformService platformService) : base(workbook, navigationService)
        {
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));

            this.platformService = platformService;
            this.items = new ObservableCollection<HeadedSettingItemViewModel>
            {
                // uservoice
                new HeadedSettingItemViewModel(
                    AppIconType.CommonGitHub,
                    StringResources.About_GetTheCode, 
                    Constants.GitHubAddress,
                    async () => await this.platformService.OpenWebUri(Constants.GitHubAddress)),
                
                // marketplace
                new HeadedSettingItemViewModel(
                    AppIconType.CommonStore,
                    StringResources.About_ShareEnthousiasm, 
                    StringResources.About_ReviewTheApp,
                    async () => await this.platformService.OpenWebUri(Constants.MarketplaceReviewAppUri)),
                
                // changelog
                new HeadedSettingItemViewModel(
                    AppIconType.CommonRefresh,
                    StringResources.About_Updates, 
                    StringResources.About_UpdatesDetail,
                    async () => await this.platformService.OpenWebUri(Constants.WindowsChangelogAddress)),

                // help
                new HeadedSettingItemViewModel(
                    AppIconType.CommonQuestion,
                    StringResources.About_Help,
                    StringResources.About_HelpDetail,
                    async () => await this.platformService.OpenWebUri(Constants.HelpPageAddress))                
            };
        }
    }
}
