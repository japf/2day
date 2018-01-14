using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Model.View;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    [DebuggerDisplay("Name: {Folder.Name}")]
    public class FolderItemViewModel : MenuItemViewModel
    {
        #region fields

        private static readonly Func<ITask, string, bool> searchTaskPredicate; 

        private readonly IAbstractFolder folder;
        private readonly IWorkbook workbook;

        private readonly ICommand setTargetGroupCommand;
        private readonly ICommand contextualCommand;

        private readonly SmartCollection<ITask> smartCollection;

        private readonly List<ITask> trackedTasks;
        private readonly ISettings settings;

        private bool hideCompletedTasks;
        private bool showFutureStartDates;

        private readonly ViewKind viewKind;
        private bool isDatesGrouped;
        private bool isSelected;

        #endregion

        #region properties

        public override string Name
        {
            get { return this.Folder.Name; }
        }

        public ICommand SetTargetGroupCommand
        {
            get { return this.setTargetGroupCommand; }
        }

        public override ICommand ContextualCommand
        {
            get { return this.contextualCommand; }
        }

        public int Count
        {
            get { return this.Folder.TaskCount; }
        }

        public override IAbstractFolder Folder
        {
            get { return this.folder; }
        }

        public override SmartCollection<ITask> SmartCollection
        {
            get { return this.smartCollection; }
        }

        public TaskGroup SelectedTaskGroup
        {
            get
            {
                return this.folder.TaskGroup;
            }
            set
            {
                if (this.folder.TaskGroup != value)
                {
                    this.folder.UpdateGroupingMode(value, this.SelectedTaskOrdering == SortOrder.Ascending);
                    this.RaisePropertyChanged("SelectedTaskGroup");
                }
            }
        }

        public SortOrder SelectedTaskOrdering
        {
            get
            {
                return this.folder.GroupAscending ? SortOrder.Ascending : SortOrder.Descending;
            }
            set
            {
                bool isAscending = value == SortOrder.Ascending;
                if (this.folder.GroupAscending != isAscending)
                {
                    this.folder.UpdateGroupingMode(this.folder.TaskGroup, isAscending);
                    this.RaisePropertyChanged("SelectedTaskOrdering");
                }
            }
        }
        
        public bool DisplayFolder
        {
            get
            {
                // if workbook only has 1 folder, it does not make sense to display it
                return !(this.folder is IFolder) && this.workbook.Folders.Count > 1;
            }
        }

        public bool DisplayDue
        {
            get
            {
                ISystemView view = this.folder as ISystemView;
                bool isTodayOrTomorrow = view != null && (view.ViewKind == ViewKind.Today || view.ViewKind == ViewKind.Tomorrow);

                return (this.folder.TaskGroup != TaskGroup.DueDate && !isTodayOrTomorrow) || this.isDatesGrouped;
            }
        }

        public override bool IsSelected  
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged("IsSelected");
                }
            }
        }

        public override bool HasContextualCommand
        {
            get { return this.folder.HasCustomCommand && this.folder.TaskCount > 0; }
        }

        #endregion

        static FolderItemViewModel()
        {
            searchTaskPredicate = ViewSearch.GetTaskSearchPredicate();
        }

        public FolderItemViewModel(IWorkbook workbook, IAbstractFolder folder)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            this.workbook = workbook;
            this.settings = workbook.Settings;
            this.settings.KeyChanged += this.OnSettingsKeyChanged;
            this.folder = folder;

            this.setTargetGroupCommand = new RelayCommand<string>(taskGroup => this.SelectedTaskGroup = TaskGroupConverter.FromName(taskGroup));
            this.contextualCommand = new RelayCommand(this.ContextualCommandExecute);

            this.showFutureStartDates = this.workbook.Settings.GetValue<bool>(CoreSettings.ShowFutureStartDates);
            this.isDatesGrouped = this.workbook.Settings.GetValue<bool>(CoreSettings.UseGroupedDates);
            this.hideCompletedTasks = this.workbook.Settings.GetValue<CompletedTaskMode>(CoreSettings.CompletedTasksMode) == CompletedTaskMode.Hide;

            ISystemView view = this.folder as ISystemView;
            if (view != null)
                this.viewKind = view.ViewKind;
            else if (this.folder is ViewSearch)
                this.viewKind = ViewKind.Search;
            else
                this.viewKind = ViewKind.None;

            this.trackedTasks = new List<ITask>();

            this.folder.TaskAdded += (s, e) => this.AddTask(e.Item);
            this.folder.TaskRemoved += (s, e) => this.RemoveTask(e.Item);

            this.folder.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Name")
                    this.RaisePropertyChanged("Name");
                else if (e.PropertyName == "TaskCount" && this.folder.HasCustomCommand)
                    this.RaisePropertyChanged("HasContextualCommand");
            };

            this.folder.GroupingChanged += (s, e) =>
            {
                this.smartCollection.GroupBuilder = GroupBuilderFactory.GetGroupBuilder(this.folder, this.settings);
                this.RaisePropertyChanged("DisplayFolder");
                this.RaisePropertyChanged("DisplayDue");
            };

            // the last parameter is a function which returns the folder associated with a task
            // when we group by folder, it's the folder of the task
            // otherwise (most of the time), it's the folder of this ViewModel
            this.smartCollection = new SmartCollection<ITask>(
                this.folder.Tasks,
                GroupBuilderFactory.GetGroupBuilder(this.folder, this.workbook.Settings),
                this.TaskFilter,
                this,
                task => this.folder);

            foreach (var task in this.folder.Tasks)
                this.StartTrackingTask(task);
        }

        private async void ContextualCommandExecute()
        {
            if (this.Folder.HasCustomCommand)
                await this.Folder.ExecuteCustomCommandAsync();
        }

        public void Rebuild()
        {
            if (this.folder is IView)
            {
                ((IView)this.folder).Rebuild();
            }

            this.smartCollection.Rebuild();
        }

        private void OnSettingsKeyChanged(object sender, SettingsKeyChanged e)
        {
            string key = e.Key;

            if (key == CoreSettings.UseGroupedDates)
            {
                this.smartCollection.GroupBuilder = GroupBuilderFactory.GetGroupBuilder(this.folder, this.workbook.Settings);
                this.isDatesGrouped = this.workbook.Settings.GetValue<bool>(CoreSettings.UseGroupedDates);
            }
            else if (key.StartsWith("TaskOrdering"))
            {
                this.smartCollection.GroupBuilder = GroupBuilderFactory.GetGroupBuilder(this.folder, this.workbook.Settings);
                this.RaisePropertyChanged("DisplayFolder");
                this.RaisePropertyChanged("DisplayDue");
            }
            else if (key == CoreSettings.CompletedTasksMode)
            {
                this.hideCompletedTasks = this.workbook.Settings.GetValue<CompletedTaskMode>(CoreSettings.CompletedTasksMode) == CompletedTaskMode.Hide;
                this.smartCollection.GroupBuilder = GroupBuilderFactory.GetGroupBuilder(this.folder, this.workbook.Settings);
                this.smartCollection.Rebuild();
            }
            else if (key == CoreSettings.ShowFutureStartDates)
            {
                this.showFutureStartDates = this.workbook.Settings.GetValue<bool>(CoreSettings.ShowFutureStartDates);
                this.smartCollection.Rebuild();
            }
        }

        private void AddTask(ITask task)
        {
            this.smartCollection.Add(task);
            this.StartTrackingTask(task);
        }

        private void RemoveTask(ITask task)
        {
            this.smartCollection.Remove(task);
            this.StopTrackingTask(task);
        }

        private void StartTrackingTask(ITask task)
        {
            if (!this.trackedTasks.Contains(task))
            {
                task.PropertyChanged += this.OnTaskPropertyChanged;
                this.trackedTasks.Add(task);
            }
        }

        private void StopTrackingTask(ITask task)
        {
            if (this.trackedTasks.Contains(task))
            {
                task.PropertyChanged -= this.OnTaskPropertyChanged;
                this.trackedTasks.Remove(task);
            }
        }

        private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Due" || e.PropertyName == "Priority" || e.PropertyName == "IsCompleted" || e.PropertyName == "Title"
                || e.PropertyName == "Folder" || e.PropertyName == "Action" || e.PropertyName == "Progress" || e.PropertyName == "Context"
                || e.PropertyName == "Modified" || e.PropertyName == "Start")
            {
                ITask task = (ITask)sender;

                // this test is needed because when a task is deleted we might have a property change
                // during the deletion of the task (before we actually have the notification saying that
                // the task has been deleted)
                if (task.Folder != null)
                {
                    this.smartCollection.InvalidateItem(task);
                }
            }
        }

        private bool TaskFilter(ITask task)
        {
            DateTime now = DateTime.Now;
            if (StaticTestOverrides.Now.HasValue)
                now = StaticTestOverrides.Now.Value;

            // this function determine whether a task is visible or not in the smart collection
            if (task.ParentId.HasValue)
            {
                // subtasks are always hidden
                return false;
            }
            else if (this.viewKind == ViewKind.Search)
            {
                // always shows tasks in that case, filtering is done in the ViewSearch implementation
                return true;
            }
            else if (this.hideCompletedTasks && this.viewKind != ViewKind.Completed && this.viewKind != ViewKind.NonCompleted && task.IsCompleted 
                && (!(this.folder is ISmartView) || !((ISmartView) this.folder).ShowCompletedTasks))
            {
                // "hide completed tasks" option is ON
                // and the selected view is not "Completed" nor "Non completed"
                // and the task is completed
                // and the selected view is not a SmartView that must show completed tasks
                // => HIDE the task
                return false;
            } 
            else if (!this.showFutureStartDates && this.viewKind != ViewKind.StartDate && task.Start.HasValue && task.Start > now)
            {
                // "show task with future start date" option is OFF
                // and the selected view is not "Start Date"
                // and the task has a start date that is after now
                // => HIDE the task
                return false;
            }
            else if (!this.showFutureStartDates && this.viewKind == ViewKind.StartDate && !task.Start.HasValue)
            {
                // "show task with future start date" option is OFF
                // the view is "Start Date" and the the task has not start date
                // => HIDE the task
                return false;
            }
            else
            {
                return true;
            }
        }
        
        public override string ToString()
        {
            return this.Name;
        }
    }
}