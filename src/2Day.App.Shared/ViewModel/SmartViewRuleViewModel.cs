using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model.Smart;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class SmartViewRuleViewModel : ViewModelBase
    {
        private readonly SmartViewBlockViewModel parent;
        private readonly ICommand removeRuleCommand;

        private SmartViewField selectedField;
        private SmartViewFilter selectedFilter;
        private string suffix;
        private SmartViewRuleValueViewModel value;

        private readonly ObservableCollection<SmartViewField> fields;
        private readonly ObservableCollection<SmartViewFilter> filters;

        public ICommand RemoveRuleCommand
        {
            get { return this.removeRuleCommand; }
        }

        public bool IsNotFirst
        {
            get { return this.parent.Rules.Count > 1 && this.parent.Rules[0] != this; }
        }
        
        public SmartViewBlockViewModel Parent
        {
            get { return this.parent; }
        }

        public IEnumerable<SmartViewField> Fields
        {
            get { return this.fields; }
        }

        public SmartViewField SelectedField
        {
            get { return this.selectedField; }
            set
            {
                if (this.selectedField != value)
                {
                    this.selectedField = value;
                    this.UpdateEditMode();

                    this.selectedFilter = this.filters.FirstOrDefault();

                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        public string Suffix
        {
            get
            {
                return this.suffix;
            }
            private set
            {
                if (this.suffix != value)
                {
                    this.suffix = value;
                    this.RaisePropertyChanged("Suffix");
                }
            }
        }

        public SmartViewRuleValueViewModel Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    if (this.value != null)
                        this.value.PropertyChanged -= this.OnValueChanged;

                    this.value = value;

                    if (this.value != null)
                        this.value.PropertyChanged += this.OnValueChanged;

                    this.RaisePropertyChanged("Value");
                    this.RaisePropertyChanged("CanEditValue");
                }
            }
        }

        public bool CanEditValue
        {
            get { return this.value != null && this.value.EditType != SmartViewEditType.None; }
        }

        public ObservableCollection<SmartViewFilter> Filters
        {
            get { return this.filters; }
        }

        public SmartViewFilter SelectedFilter
        {
            get { return this.selectedFilter; }
            set
            {
                if (this.selectedFilter != value)
                {
                    this.selectedFilter = value;
                    this.UpdateEditMode();

                    this.RaisePropertyChanged("SelectedFilter");
                }
            }
        }

        public SmartViewRuleViewModel(SmartViewBlockViewModel parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            this.parent = parent;

            this.fields = new ObservableCollection<SmartViewField>(EnumHelper.GetAllValues<SmartViewField>());
            this.filters = new ObservableCollection<SmartViewFilter>();

            this.removeRuleCommand = new RelayCommand(this.RemoveRuleExecute);

            this.UpdateEditMode();
        }

        private void OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged("OnValueChanged");
        }

        private void UpdateEditMode()
        {
            this.filters.Clear();
            this.filters.AddRange(SmartViewHelper.GetCompatibleFilter(this.SelectedField));

            this.Value = this.parent.Parent.CreateValueViewModel(this.SelectedFilter, this.SelectedField, null);
            this.Suffix = SmartViewHelper.GetEditMode(this.SelectedFilter, this.SelectedField).Suffix;
        }

        public SmartViewRule BuildRule()
        {
            return SmartViewHelper.BuildRule(this.SelectedFilter, this.SelectedField, this.Value.Value);
        }

        private void RemoveRuleExecute()
        {
            SmartViewBlockViewModel block = this.parent;
            block.Rules.Remove(this);
            if (block.Rules.Count == 0)
                block.Parent.Blocks.Remove(block);
        }        
    }
}