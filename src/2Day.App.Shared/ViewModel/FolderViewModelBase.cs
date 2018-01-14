using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public abstract class FolderViewModelBase : PageViewModelBase
    {
        #region fields

        private readonly IMessageBoxService messageBoxService;
        private readonly IPlatformService platformService;
        private readonly List<int> availableIcons;
        private readonly List<ItemCountViewModel> accentColors;

        private string title;
        private int selectedIcon;
        private int selectedColorIndex;

        private TaskGroup taskGroup;
        private bool isAscending;
        private bool showInViews;

        #endregion

        #region properties
        
        public bool IsAscending
        {
            get
            {
                return this.isAscending;
            }
            set
            {
                this.isAscending = value;
                this.RaisePropertyChanged("IsDescending");
                this.RaisePropertyChanged("IsAscending");
            }
        }

        public bool IsDescending
        {
            get
            {
                return !this.isAscending;
            }
            set
            {
                this.isAscending = !value;
                this.RaisePropertyChanged("IsDescending");
                this.RaisePropertyChanged("IsAscending");
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaisePropertyChanged("Title");
                }
            }
        }

        public List<int> AvailableIcons
        {
            get
            {
                return this.availableIcons;
            }
        }

        public int SelectedIcon
        {
            get
            {
                return this.selectedIcon;
            }
            set
            {
                if (this.selectedIcon != value)
                {
                    this.selectedIcon = value;
                    this.RaisePropertyChanged("SelectedIcon");
                }
            }
        }

        public List<ItemCountViewModel> AccentColors
        {
            get { return this.accentColors; }
        }

        public int SelectedColorIndex
        {
            get
            {
                return this.selectedColorIndex;
            }
            set
            {
                if (this.selectedColorIndex != value)
                {
                    this.selectedColorIndex = value;

                    this.RaisePropertyChanged("SelectedColorIndex");
                    this.RaisePropertyChanged("SelectedColor");
                }
            }
        }

        public string SelectedColor
        {
            get
            {
                if (this.selectedColorIndex > -1 && this.selectedColorIndex < this.AccentColors.Count)
                    return this.AccentColors[this.selectedColorIndex].Name;
                else
                    return string.Empty;
            }
        }

        public TaskGroup TaskGroup
        {
            get { return this.taskGroup; }
            protected set
            {
                if (this.taskGroup != value)
                {
                    this.taskGroup = value;
                    this.RaisePropertyChanged("TaskGroup");
                }
            }
        }

        public bool ShowInViews
        {
            get { return this.showInViews; }
            set
            {
                if (this.showInViews != value)
                {
                    this.showInViews = value;
                    this.RaisePropertyChanged("ShowInViews");
                }
            }
        }

        public virtual bool CanChangeShowInViews
        {
            get { return true; }
        }

        public abstract string PageTitle
        {
            get;
        }
        
        protected IMessageBoxService MessageBoxService
        {
            get { return this.messageBoxService; }
        }

        protected IPlatformService PlatformService
        {
            get { return this.platformService; }
        }

        protected virtual IEnumerable<IFolder> OtherFolders
        {
            get { return this.Workbook.Folders; }
        }

        public bool IsGroupAction
        {
            get { return this.taskGroup == TaskGroup.Action; }
            set
            {
                if (this.taskGroup != TaskGroup.Action)
                {
                    this.taskGroup = TaskGroup.Action;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupContext
        {
            get { return this.taskGroup == TaskGroup.Context; }
            set
            {
                if (this.taskGroup != TaskGroup.Context)
                {
                    this.taskGroup = TaskGroup.Context;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupFolder
        {
            get { return this.taskGroup == TaskGroup.Folder; }
            set
            {
                if (this.taskGroup != TaskGroup.Folder)
                {
                    this.taskGroup = TaskGroup.Folder;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupPriority
        {
            get { return this.taskGroup == TaskGroup.Priority; }
            set
            {
                if (this.taskGroup != TaskGroup.Priority)
                {
                    this.taskGroup = TaskGroup.Priority;
                    this.IsDescending = true;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupDueDate
        {
            get { return this.taskGroup == TaskGroup.DueDate; }
            set
            {
                if (this.taskGroup != TaskGroup.DueDate)
                {
                    this.taskGroup = TaskGroup.DueDate;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupProgress
        {
            get { return this.taskGroup == TaskGroup.Progress; }
            set
            {
                if (this.taskGroup != TaskGroup.Progress)
                {
                    this.taskGroup = TaskGroup.Progress;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupStartDate
        {
            get { return this.taskGroup == TaskGroup.StartDate; }
            set
            {
                if (this.taskGroup != TaskGroup.StartDate)
                {
                    this.taskGroup = TaskGroup.StartDate;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupStatus
        {
            get { return this.taskGroup == TaskGroup.Status; }
            set
            {
                if (this.taskGroup != TaskGroup.Status)
                {
                    this.taskGroup = TaskGroup.Status;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupCompleted
        {
            get { return this.taskGroup == TaskGroup.Completed; }
            set
            {
                if (this.taskGroup != TaskGroup.Completed)
                {
                    this.taskGroup = TaskGroup.Completed;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public bool IsGroupModified
        {
            get { return this.taskGroup == TaskGroup.Modified; }
            set
            {
                if (this.taskGroup != TaskGroup.Modified)
                {
                    this.taskGroup = TaskGroup.Modified;
                    this.IsDescending = false;
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        #endregion

        protected FolderViewModelBase(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, IPlatformService platformService) : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));

            this.messageBoxService = messageBoxService;
            this.platformService = platformService;

            this.availableIcons = new List<int>(Enumerable.Range(1, FontIconHelper.FolderIconsCount));
            this.accentColors = new List<ItemCountViewModel>(ColorChooser.Colors.Select(c => new ItemCountViewModel(c, this.GetColorUsage(c))));
        }

        private int GetColorUsage(string color)
        {
            return this.Workbook.Folders.Count(f => f.Color.Equals(color, StringComparison.OrdinalIgnoreCase));
        }

        protected override async Task<bool> CancelGoBackAsync()
        {
            if (this.HasDirtyChanges())
            {
                var result = await this.messageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_LooseChangesContent, DialogButton.YesNo);
                return result != DialogResult.Yes;
            }
            else
            {
                return false;
            }
        }

        protected abstract bool HasDirtyChanges();

        protected virtual bool DeleteCanExecute()
        {
            return false;
        }

        protected void UpdateFolder(IAbstractFolder target)
        {
            var folder = target as IFolder;

            if (this.selectedColorIndex < 0)
                this.selectedColorIndex = 0;

            if (folder != null)
            {
                folder.Name = this.Title;
                folder.Color = ColorChooser.Colors[this.selectedColorIndex];
                folder.IconId = this.SelectedIcon;
                if (folder.IconId < 1)
                    folder.IconId = 1;
                folder.ShowInViews = this.ShowInViews;
            }

            target.UpdateGroupingMode(this.TaskGroup, this.isAscending);
        }

        protected async Task<bool> CheckTitleAsync()
        {
            this.Title = this.Title.TryTrim();
            if (string.IsNullOrEmpty(this.Title))
            {
                await this.MessageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.Message_TitleCannotBeEmpty);
                return false;
            }

            // make sure the proposed named is valid and is not already used by another folder
            if (this.OtherFolders.Any(f => this.Title.Equals(f.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                await this.MessageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.CreateEditFolder_FolderNameAlreadyUsedNotification);
                return false;
            }

            return true;
        }

        public void Refresh()
        {
            this.RaisePropertyChanged("IsGroupAction");
            this.RaisePropertyChanged("IsGroupContext");
            this.RaisePropertyChanged("IsGroupFolder");
            this.RaisePropertyChanged("IsGroupPriority");
            this.RaisePropertyChanged("IsGroupDueDate");
            this.RaisePropertyChanged("IsGroupProgress");
            this.RaisePropertyChanged("IsGroupStartDate");
            this.RaisePropertyChanged("IsGroupStatus");
            this.RaisePropertyChanged("IsGroupCompleted");
            this.RaisePropertyChanged("IsGroupModified");

            this.RaisePropertyChanged("IsAscending");
            this.RaisePropertyChanged("IsDescending");
        }

    }
}