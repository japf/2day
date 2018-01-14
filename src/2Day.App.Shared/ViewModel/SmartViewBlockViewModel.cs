using System.Collections.ObjectModel;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model.Smart;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class SmartViewBlockViewModel : ViewModelBase
    {
        private readonly SmartViewViewModelBase parent;
        private readonly ObservableCollection<SmartViewRuleViewModel> rules;
        private readonly ICommand addRuleCommand;
        private SmartViewMatchType matchType;

        public ObservableCollection<SmartViewRuleViewModel> Rules
        {
            get { return this.rules; }
        }

        public ICommand AddRuleCommand
        {
            get { return this.addRuleCommand; }
        }

        public SmartViewMatchType MatchType
        {
            get { return this.matchType; }
        }
        
        public bool MatchAnd
        {
            get { return this.matchType == SmartViewMatchType.All; }
            set
            {
                if (this.matchType != SmartViewMatchType.All)
                {
                    this.matchType = SmartViewMatchType.All;
                    this.RaisePropertyChanged("MatchAnd");
                    this.RaisePropertyChanged("MatchOr");
                }
            }
        }

        public bool MatchOr
        {
            get { return this.matchType == SmartViewMatchType.Any; }
            set
            {
                if (this.matchType != SmartViewMatchType.Any)
                {
                    this.matchType = SmartViewMatchType.Any;
                    this.RaisePropertyChanged("MatchAnd");
                    this.RaisePropertyChanged("MatchOr");
                }
            }
        }

        public bool IsNotFirst
        {
            get { return this.Parent.Blocks.Count > 1 && this.Parent.Blocks[0] != this; }
        }

        public SmartViewViewModelBase Parent
        {
            get { return this.parent; }
        }

        public SmartViewBlockViewModel(SmartViewViewModelBase parent)
        {
            this.parent = parent;

            this.rules = new ObservableCollection<SmartViewRuleViewModel>();

            this.addRuleCommand = new RelayCommand(this.AddRuleExecute);
        }

        private void AddRuleExecute()
        {
            this.Rules.Add(new SmartViewRuleViewModel(this));
        }        
    }
}