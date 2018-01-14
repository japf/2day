using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class EditViewViewModel : FolderViewModelBase
    {
        private readonly ISystemView view;

        public override string PageTitle
        {
            get { return string.Empty; } // page header is not visible
        }

        public EditViewViewModel(ISystemView view, IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, IPlatformService platformService) 
            : base(workbook, navigationService, messageBoxService, platformService)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            this.view = view;

            this.IsAscending = view.GroupAscending;
            this.TaskGroup = view.TaskGroup;
        }

        protected override bool HasDirtyChanges()
        {
            return false;
        }

        protected override void SaveExecute()
        {
            this.UpdateFolder(this.view);
        }

        public override void Dispose()
        {
            this.SaveCommand.Execute(null);
        }
    }
}