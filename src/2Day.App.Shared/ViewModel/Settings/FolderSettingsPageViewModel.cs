using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class FolderSettingsPageViewModel : CollectionViewSourceSettingsViewModel
    {
        private ObservableCollection<IFolder> folders;

        public FolderSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService)
            : base(workbook, navigationService, messageBoxService)
        {
        }

        protected override INotifyCollectionChanged GetSource()
        {
            if (this.folders == null)
                this.folders = new ObservableCollection<IFolder>(this.Workbook.Folders);

            return this.folders;
        }

        protected override void CreateItemExecute(string parameter)
        {
            this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage);
        }

        protected override async void RemoveItemExecute(object parameter)
        {
            if (parameter is IFolder)
            {
                var folder = (IFolder) parameter;
                var result = await this.MessageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteFolderText, DialogButton.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.Workbook.RemoveFolder(folder.Name);
                    this.folders.Remove(folder);
                }
            }
        }

        protected override void EditItemExecute(object parameter)
        {
            if (parameter is IFolder)
            {
                var smartview = (IFolder)parameter;
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage, smartview);
            }
        }

        protected override void OnOrderChanged()
        {
            this.Workbook.ApplyFolderOrder(new List<IFolder>(this.folders));
        }
    }
}
