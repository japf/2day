using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class EditFolderViewModel : FolderViewModelBase
    {
        private readonly IAbstractFolder abstractFolder;
        private readonly ITileManager tileManager;

        public override string PageTitle
        {
            get
            {
                if (this.abstractFolder is IFolder)
                    return StringResources.EditFolderPage_Title;
                else
                    return StringResources.EditViewPage_Title;
            }
        }

        public override bool CanDelete
        {
            get { return true; }
        }

        public override bool CanPin
        {
            get { return true; }
        }

        public override bool IsPinned
        {
            get { return this.tileManager.IsPinned(this.abstractFolder); }
        }

        protected override IEnumerable<IFolder> OtherFolders
        {
            get { return this.Workbook.Folders.Where(f => f != this.abstractFolder); }
        }

        public override bool CanChangeShowInViews
        {
            get { return this.abstractFolder is IFolder; }
        }

        public EditFolderViewModel(IAbstractFolder abstractFolder, IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, IPlatformService platformService, ITileManager tileManager)
            : base(workbook, navigationService, messageBoxService, platformService)
        {
            if (abstractFolder == null)
                throw new ArgumentNullException("abstractFolder");
            if (tileManager == null)
                throw new ArgumentNullException("tileManager");

            this.abstractFolder = abstractFolder;
            this.tileManager = tileManager;

            this.Title = abstractFolder.Name;
            this.IsAscending = abstractFolder.GroupAscending;
            this.TaskGroup = abstractFolder.TaskGroup;

            this.SelectedColorIndex = ColorChooser.GetColorIndex(abstractFolder.Color);
            this.SelectedIcon = abstractFolder.IconId;

            if (abstractFolder is IFolder)
            {
                var folder = (IFolder) abstractFolder;
                this.ShowInViews = folder.ShowInViews.HasValue && folder.ShowInViews.Value;
            }
        }

        protected override async void PinExecute()
        {
            if (this.IsPinned)
                await this.tileManager.UnpinAsync(this.abstractFolder);
            else
                await this.tileManager.PinAsync(this.abstractFolder);

            this.RaisePropertyChanged("IsPinned");
        }

        protected override async void SaveExecute()
        {
            bool isTitleValid = true;
            if (this.abstractFolder is IFolder)
                isTitleValid = await this.CheckTitleAsync();

            if (isTitleValid)
            {
                this.UpdateFolder(this.abstractFolder);
                this.NavigationService.GoBack();
            }
        }

        protected override bool DeleteCanExecute()
        {
            return true;
        }

        protected override bool HasDirtyChanges()
        {
            if (this.abstractFolder is IFolder)
            {
                var folder = (IFolder) this.abstractFolder;
                return !folder.IsEqualsTo(this.Title, this.SelectedColor, this.TaskGroup, this.IsAscending, this.ShowInViews);
            }

            return false;
        }

        protected override async void DeleteExecute()
        {
            var result = await this.MessageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteFolderText, DialogButton.YesNo);
            if (result == DialogResult.Yes)
            {
                this.Workbook.RemoveFolder(this.abstractFolder.Name);
                this.NavigationService.GoBack();
            }
        }
    }
}