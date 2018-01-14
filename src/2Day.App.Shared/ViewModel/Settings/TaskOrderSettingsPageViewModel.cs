using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class TaskOrderSettingsPageViewModel : PageViewModelBase
    {
        private string sortOption1;
        private bool isAscending1;
        private string sortOption2;
        private bool isAscending2;
        private string sortOption3;
        private bool isAscending3;

        public IEnumerable<string> AvailableSortOptions
        {
            get { return TaskOrderingConverter.Descriptions; }
        }

        public string SortOption1
        {
            get
            {
                return this.sortOption1;
            }
            set
            {
                if (this.sortOption1 != value)
                {
                    this.sortOption1 = value;
                    this.RaisePropertyChanged("SortOption1");
                }
            }
        }

        public bool IsAscending1
        {
            get
            {
                return this.isAscending1;
            }
            set
            {
                if (this.isAscending1 != value)
                {
                    this.isAscending1 = value;
                    this.RaisePropertyChanged("IsAscending1");
                    this.RaisePropertyChanged("IsDescending1");
                }
            }
        }

        public string SortOption2
        {
            get
            {
                return this.sortOption2;
            }
            set
            {
                if (this.sortOption2 != value)
                {
                    this.sortOption2 = value;
                    this.RaisePropertyChanged("SortOption2");
                }
            }
        }

        public bool IsAscending2
        {
            get
            {
                return this.isAscending2;
            }
            set
            {
                if (this.isAscending2 != value)
                {
                    this.isAscending2 = value;
                    this.RaisePropertyChanged("IsAscending2");
                }
            }
        }

        public string SortOption3
        {
            get
            {
                return this.sortOption3;
            }
            set
            {
                if (this.sortOption3 != value)
                {
                    this.sortOption3 = value;
                    this.RaisePropertyChanged("SortOption3");
                }
            }
        }

        public bool IsAscending3
        {
            get
            {
                return this.isAscending3;
            }
            set
            {
                if (this.isAscending3 != value)
                {
                    this.isAscending3 = value;
                    this.RaisePropertyChanged("IsAscending3");
                }
            }
        }

        public TaskOrderSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService) : base(workbook, navigationService)
        {
            TaskOrdering taskOrdering1 = this.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType1);
            this.sortOption1 = taskOrdering1.GetDescription();
            this.isAscending1 = this.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending1);

            TaskOrdering taskOrdering2 = this.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType2);
            this.sortOption2 = taskOrdering2.GetDescription();
            this.isAscending2 = this.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending2);

            TaskOrdering taskOrdering3 = this.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType3);
            this.sortOption3 = taskOrdering3.GetDescription();
            this.isAscending3 = this.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending3);
        }

        public override void Dispose()
        {
            TaskOrdering ordering1 = TaskOrderingConverter.FromDescription(this.sortOption1);
            if (this.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType1) != ordering1)
                this.Settings.SetValue(CoreSettings.TaskOrderingType1, ordering1);

            if (this.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending1) != this.IsAscending1)
                this.Settings.SetValue(CoreSettings.TaskOrderingAscending1, this.IsAscending1);

            TaskOrdering ordering2 = TaskOrderingConverter.FromDescription(this.sortOption2);
            if (this.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType2) != ordering2)
                this.Settings.SetValue(CoreSettings.TaskOrderingType2, ordering2);

            if (this.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending2) != this.IsAscending2)
                this.Settings.SetValue(CoreSettings.TaskOrderingAscending2, this.IsAscending2);

            TaskOrdering ordering3 = TaskOrderingConverter.FromDescription(this.sortOption3);
            if (this.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType3) != ordering3)
                this.Settings.SetValue(CoreSettings.TaskOrderingType3, ordering3);

            if (this.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending3) != this.IsAscending3)
                this.Settings.SetValue(CoreSettings.TaskOrderingAscending3, this.IsAscending3);
        }
    }
}
