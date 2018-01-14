using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class CreateFolderViewModel : FolderViewModelBase
    {
        public override string PageTitle
        {
            get { return StringResources.CreateFolderPage_Title; }
        }

        public CreateFolderViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, IPlatformService platformService)
            : base(workbook, navigationService, messageBoxService, platformService)
        {
            this.ShowInViews = true;
            this.SelectedIcon = 1;
        }

        protected override bool HasDirtyChanges()
        {
            return !string.IsNullOrEmpty(this.Title);
        }

        protected override async void SaveExecute()
        {
            if (await this.CheckTitleAsync())
            {
                var folder = this.Workbook.AddFolder(this.Title);

                this.UpdateFolder(folder);

                this.NavigationService.GoBack();
            }
        }
    }
}
