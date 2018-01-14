using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Smart;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public abstract class SmartViewViewModelBase : PageViewModelBase, IPageViewModel, IDisposable
    {
        private readonly IWorkbook workbook;
        private readonly INavigationService navigationService;
        private readonly IMessageBoxService messageBoxService;
        private readonly ITrackingManager trackingManager;

        private readonly ObservableCollection<SmartViewBlockViewModel> blocks;

        private readonly ICommand addBlockCommand;

        private string name;
        private string ruleContent;
        private SmartViewMatchType matchType;
        private int matchCount;

        private readonly FolderItemViewModel liveCountFolderViewModel;
        private readonly SmartViewViewModelTracker tracker;

        public ICommand AddBlockCommand
        {
            get { return this.addBlockCommand; }
        }
        
        public int MatchCount
        {
            get { return this.matchCount; }
            private set
            {
                if (this.matchCount != value)
                {
                    this.matchCount = value;
                    this.RaisePropertyChanged("MatchCount");
                }
            }
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

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        public string RuleContent
        {
            get { return this.ruleContent; }
            set
            {
                if (this.ruleContent != value)
                {
                    this.ruleContent = value;
                    this.RaisePropertyChanged("RuleContent");
                }
            }
        }
        
        public ObservableCollection<SmartViewBlockViewModel> Blocks
        {
            get { return this.blocks; }
        }

        protected ITrackingManager TrackingManager
        {
            get { return this.trackingManager; }
        }

        protected SmartViewViewModelBase(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ITrackingManager trackingManager)
            : base(workbook, navigationService)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.workbook = workbook;
            this.navigationService = navigationService;
            this.messageBoxService = messageBoxService;
            this.trackingManager = trackingManager;

            this.blocks = new ObservableCollection<SmartViewBlockViewModel>();

            this.addBlockCommand = new RelayCommand(this.AddBlockExecute);

            var smartview = new SmartView(this.workbook, new Core.Shared.Model.Impl.SmartView() { Rules = "(Title Contains azerty)"});
            this.liveCountFolderViewModel = new FolderItemViewModel(this.workbook, smartview);

            this.tracker = new SmartViewViewModelTracker(this);
            this.tracker.Changed += this.OnContentChanged;
        }

        private void OnContentChanged(object sender, EventArgs e)
        {
            try
            {
                string rule = this.BuildString();
                if (!string.IsNullOrWhiteSpace(rule))
                {
                    ((SmartView) this.liveCountFolderViewModel.Folder).Rules = rule;
                    this.MatchCount = this.liveCountFolderViewModel.SmartCollection.Count;
                }
            }
            catch (Exception)
            {
                // rule can be invalid when user is stil configuring it
                // so we don't care about those errors and just ignore them
                this.MatchCount = 0;
            }                
        }

        private void AddBlockExecute()
        {
            var blockViewModel = new SmartViewBlockViewModel(this);
            blockViewModel.Rules.Add(new SmartViewRuleViewModel(blockViewModel));

            this.blocks.Add(blockViewModel);
        }

        protected override async void SaveExecute()
        { 
            if (string.IsNullOrEmpty(this.Name))
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.Message_TitleCannotBeEmpty);
                return;
            }

            if (this.Blocks.Count == 0)
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.SmartView_Warning_OneBlockMin);
                return;
            }

            if (this.Blocks.All(b => b.Rules.Count == 0))
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.SmartView_Warning_OneRuleMin);
                return;
            }

            bool success = false;
            Exception exception = null;
            try
            {
                this.RuleContent = this.BuildString();
                success = this.SaveCore();
                if (success)
                    this.navigationService.GoBack();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null || !success)
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.SmartView_MessageCannotSave);
            }
        }

        protected abstract bool SaveCore();

        protected async void LoadStringAsync(string content)
        {
            Exception exception = null;
            try
            {
                var handler = SmartViewHandler.FromString(content);
                foreach (var block in handler.Blocks)
                {
                    var blockViewModel = new SmartViewBlockViewModel(this);

                    foreach (var rule in block.Rules)
                    {
                        var ruleViewModel = new SmartViewRuleViewModel(blockViewModel)
                        {
                            SelectedField = rule.Field,
                            SelectedFilter = rule.Filter,
                            Value = this.CreateValueViewModel(rule.Filter, rule.Field, rule.Value)
                        };

                        blockViewModel.Rules.Add(ruleViewModel);
                    }
                    if (block.Match == SmartViewMatchType.All)
                        blockViewModel.MatchAnd = true;
                    else
                        blockViewModel.MatchOr = true;

                    this.blocks.Add(blockViewModel);
                }
                this.matchType = handler.Match;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, string.Format(StringResources.SmartView_MessageCannotLoadFormat, exception));
            }
        }

        public SmartViewRuleValueViewModel CreateValueViewModel(SmartViewFilter filter, SmartViewField field, object value)
        {
            IEnumerable<string> values = null;
            if (field == SmartViewField.Folder)
                values = this.Workbook.Folders.Select(f => f.Name);
            else if (field == SmartViewField.Context)
                values = this.workbook.Contexts.Select(f => f.Name);
            else if (field == SmartViewField.Priority)
                values = TaskPriorityConverter.Descriptions;

            if (value is SmartViewDateParameter)
            {
                var dateParameter = (SmartViewDateParameter) value;
                if (dateParameter.Date != null)
                    value = dateParameter.Date;
                else
                    value = dateParameter.Days;
            }
            else if (value is TaskPriority)
            {
                value = ((TaskPriority) value).GetDescription();
            }

            var valueViewModel = new SmartViewRuleValueViewModel(SmartViewHelper.GetEditMode(filter, field).Type, values)
            {
                Value = value
            };

            return valueViewModel;
        }

        private string BuildString()
        {
            var blockRules = new List<SmartViewBlockRule>();
            foreach (SmartViewBlockViewModel block in this.blocks)
            {
                var rules = new List<SmartViewRule>();
                foreach (SmartViewRuleViewModel rule in block.Rules)
                {
                    rules.Add(rule.BuildRule());
                }
                blockRules.Add(new SmartViewBlockRule(rules, block.MatchType));
            }

            return new SmartViewHandler(this.matchType, blockRules).AsString();
        }
        
        public override void Dispose()
        {
            this.tracker.Dispose();
        }
    }

    public class SmartViewViewModelTracker : IDisposable
    {
        private readonly SmartViewViewModelBase viewmodel;
        public event EventHandler Changed;

        public SmartViewViewModelTracker(SmartViewViewModelBase viewmodel)
        {
            if (viewmodel == null)
                throw new ArgumentNullException(nameof(viewmodel));

            this.viewmodel = viewmodel;
            this.viewmodel.PropertyChanged += this.OnPropertyChanged;
            this.viewmodel.Blocks.CollectionChanged += this.OnCollectionChanged;
            foreach (var block in this.viewmodel.Blocks)
            {
                block.PropertyChanged += this.OnPropertyChanged;
                block.Rules.CollectionChanged += this.OnCollectionChanged;
                foreach (var rule in block.Rules)
                {
                    rule.PropertyChanged += this.OnPropertyChanged;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var block in e.NewItems.OfType<SmartViewBlockViewModel>())
                {
                    block.Rules.CollectionChanged += this.OnCollectionChanged;
                    block.PropertyChanged += this.OnPropertyChanged;
                    foreach (var rule in block.Rules)
                    {
                        rule.PropertyChanged += this.OnPropertyChanged;
                    }
                }
                foreach (var rule in e.NewItems.OfType<SmartViewRuleViewModel>())
                {
                    rule.PropertyChanged += this.OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var block in e.OldItems.OfType<SmartViewBlockViewModel>())
                {
                    block.Rules.CollectionChanged -= this.OnCollectionChanged;
                    block.PropertyChanged -= this.OnPropertyChanged;
                    foreach (var rule in block.Rules)
                    {
                        rule.PropertyChanged -= this.OnPropertyChanged;
                    }
                }
                foreach (var rule in e.OldItems.OfType<SmartViewRuleViewModel>())
                {
                    rule.PropertyChanged -= this.OnPropertyChanged;
                }
            }

            this.RaiseChanged();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaiseChanged();            
        }

        private void RaiseChanged()
        {
            if (this.Changed != null)
                this.Changed(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            this.viewmodel.PropertyChanged -= this.OnPropertyChanged;
            this.viewmodel.Blocks.CollectionChanged -= this.OnCollectionChanged;
            foreach (var block in this.viewmodel.Blocks)
            {
                block.PropertyChanged -= this.OnPropertyChanged;
                block.Rules.CollectionChanged -= this.OnCollectionChanged;
                foreach (var rule in block.Rules)
                {
                    rule.PropertyChanged -= this.OnPropertyChanged;
                }
            }
        }
    }
}