using System;
using System.Collections.ObjectModel;
using System.Linq;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private readonly ISynchronizationManager syncManager;
        private readonly IPlatformService platformService;
        private readonly ObservableCollection<SettingsItemViewModel> items;

        public ObservableCollection<SettingsItemViewModel> Items
        {
            get { return this.items; }
        }

        public SettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, ISynchronizationManager syncManager, IPlatformService platformService) : base(workbook, navigationService)
        {
            if (syncManager == null)
                throw new ArgumentNullException("syncManager");
            if (platformService == null)
                throw new ArgumentNullException("platformService");

            this.syncManager = syncManager;
            this.platformService = platformService;

            this.items = new ObservableCollection<SettingsItemViewModel>
            {
                new SettingsItemViewModel(
                    AppIconType.SettingGeneral,
                    StringResources.OptionSettingsPage_SectionGeneral, 
                    this.GetGeneralDescription, 
                    () => this.FlyOutTo(ViewLocator.SettingsGeneralPage)),
                new SettingsItemViewModel(
                    AppIconType.SettingSync,
                    StringResources.OptionSettingsPage_SectionSync,
                    this.GetSyncDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsSyncPage)),
                new SettingsItemViewModel(
                    AppIconType.SettingDisplay,
                    StringResources.OptionSettingsPage_SectionDisplay, 
                    this.GetDisplayDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsDisplayPage)), 
                new SettingsItemViewModel(
                    AppIconType.SettingOrdering,
                    StringResources.OptionSettingsPage_SectionTaskOrder, 
                    this.GetTaskOrderDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsTaskOrderingPage)), 
                new SettingsItemViewModel(
                    AppIconType.SettingViews,
                    StringResources.OptionSettingsPage_SectionViews, 
                    this.GetViewsDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsViewsPage)),
                new SettingsItemViewModel(
                    AppIconType.SettingSmartViews,
                    StringResources.SmartView_LabelSmartView, 
                    this.GetSmartViewsDescription, 
                    () => this.FlyOutTo(ViewLocator.SettingsSmartViewsPage)),
                new SettingsItemViewModel(
                    AppIconType.SettingFolders,
                    StringResources.OptionSettingsPage_SectionFolders, 
                    this.GetFoldersDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsFoldersPage)), 
                new SettingsItemViewModel(
                    AppIconType.SettingContexts,
                    StringResources.OptionSettingsPage_SectionContexts, 
                    this.GetContextsDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsContextsPage)), 
                new SettingsItemViewModel(
                    AppIconType.SettingMisc,
                    StringResources.OptionSettingsPage_SectionMisc, 
                    this.GetMiscDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsMiscPage)),
                new SettingsItemViewModel(
                    AppIconType.SettingHelp,
                    StringResources.OptionSettingsPage_SectionHelp,
                    this.GetHelpDescription,
                    () => this.platformService.OpenWebUri(Constants.HelpPageAddress)),
                new SettingsItemViewModel(
                    AppIconType.SettingAbout,
                    StringResources.OptionSettingsPage_SectionAbout, 
                    this.GetAboutDescription,
                    () => this.FlyOutTo(ViewLocator.SettingsAboutPage))
            };
            
            this.NavigationService.FlyoutClosing += this.OnFlyoutClosing;
        }

        public override void Dispose()
        {
            this.NavigationService.FlyoutClosing -= this.OnFlyoutClosing;
        }

        private void OnFlyoutClosing(object sender, EventArgs e)
        {
            foreach (var item in this.items)
                item.UpdateDescription();
        }

        private void FlyOutTo(Type type)
        {
            this.NavigationService.FlyoutTo(type);
        }

        private string GetGeneralDescription()
        {
            AutoDeleteFrequency autodelete = this.Workbook.Settings.GetValue<AutoDeleteFrequency>(CoreSettings.AutoDeleteFrequency);

            return string.Format(
                StringResources.OptionSettingsPage_DescriptionTasksFormat,
                (int)autodelete,
                this.Settings.GetValue<CompletedTaskMode>(CoreSettings.CompletedTasksMode).GetDescription().ToLower());
        }

        private string GetDisplayDescription()
        {
            string themeDescription = StringResources.Settings_OptionTheme.ToLower() + " ";
            if (this.Settings.GetValue<bool>(CoreSettings.UseDarkTheme))
                themeDescription += StringResources.Settings_ThemeDark.ToLower();
            else
                themeDescription += StringResources.Settings_ThemeLight.ToLower();

            return themeDescription;
        }

        private string GetTaskOrderDescription()
        {
            TaskOrdering taskOrdering1 = this.Workbook.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType1);
            TaskOrdering taskOrdering2 = this.Workbook.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType2);
            TaskOrdering taskOrdering3 = this.Workbook.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType3);

            return string.Format(StringResources.OptionSettingsPage_SectionOrderingFormat, taskOrdering1.GetDescription(), taskOrdering2.GetDescription(), taskOrdering3.GetDescription());
        }

        private string GetFoldersDescription()
        {
            return string.Format(StringResources.OptionSettingsPage_SectionFoldersFormat, this.Workbook.Folders.Count);
        }

        private string GetContextsDescription()
        {
            return string.Format(StringResources.OptionSettingsPage_SectionContextsFormat, this.Workbook.Contexts.Count);
        }

        private string GetViewsDescription()
        {
            return string.Format(StringResources.OptionSettingsPage_SectionViewsFormat, this.Workbook.Views.Count(f => f.IsEnabled), this.Workbook.Views.Count());
        }

        private string GetSmartViewsDescription()
        {
            return string.Format(StringResources.Settings_SmartView_DescriptionFormat, this.Workbook.SmartViews.Count());            
        }

        private string GetMiscDescription()
        {
            return StringResources.OptionSettingsPage_SectionMiscFormat;
        }

        private string GetAboutDescription()
        {
            return string.Format(StringResources.OptionSettingsPage_SectionVersionFormat, this.platformService.Version);
        }

        private string GetHelpDescription()
        {
            return StringResources.OptionSettingsPage_SectionHelpFormat;
        }

        private string GetSyncDescription()
        {
            if (this.syncManager.ActiveService == SynchronizationService.None)
                return StringResources.OptionSettingsPage_SyncNoConfigured;
            else
            {
                string lastSync = StringResources.SyncSettingsPage_LastSyncNever;
                if (this.syncManager.Metadata.LastSync != DateTime.MinValue)
                    lastSync = this.syncManager.Metadata.LastSync.ToString("G");

                string providerName = this.syncManager.GetProvider(this.syncManager.ActiveService).Name;

                return string.Format(StringResources.OptionSettingsPage_SyncConfiguredFormat, providerName, lastSync);
            }
        }

        private string GetOnOff(bool value)
        {
            return value ? StringResources.OptionSettingsPage_DescriptionOn : StringResources.OptionSettingsPage_DescriptionOff;
        }

        private string GetOnOff(string settingsKey)
        {
            return this.Workbook.Settings.GetValue<bool>(settingsKey) ? StringResources.OptionSettingsPage_DescriptionOn : StringResources.OptionSettingsPage_DescriptionOff;
        }
    }
}
