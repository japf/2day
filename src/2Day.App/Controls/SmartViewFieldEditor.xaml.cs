using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class SmartViewFieldEditor : UserControl
    {
        private readonly ObservableCollection<ItemCountViewModel> availableTags;
        public ObservableCollection<string> TagsName { get; }

        public ObservableCollection<ItemCountViewModel> AvailableTags
        {
            get { return this.availableTags; }
        }

        public SmartViewFieldEditor()
        {
            this.InitializeComponent();

            // check for design time
            if (Ioc.HasType<IWorkbook>())
            {
                var workbook = Ioc.Resolve<IWorkbook>();

                this.TagsName = new ObservableCollection<string>(workbook.Tags.Select(t => t.Name));
                this.availableTags = new ObservableCollection<ItemCountViewModel>();

                IDictionary<string, int> tagUsages = workbook.GetTagsUsage();
                foreach (var tagUsage in tagUsages)
                    this.AvailableTags.Add(new ItemCountViewModel(tagUsage.Key, tagUsage.Value, new RelayCommand<string>((tag) =>
                    {
                        var context = this.DataContext as SmartViewRuleViewModel;
                        if (context != null && context.Value != null)
                            context.Value.Value = tag;
                    })));
            }

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                var children = this.LayoutRoot.Children.ToList();
                this.LayoutRoot.Children.Clear();

                var stackPanel = new StackPanel();
                foreach (var child in children)
                {
                    stackPanel.Children.Add(child);
                }
                this.Content = stackPanel;
            }
        }

        private void OnSelectedFieldChanged(object sender, RoutedEventArgs e)
        {
            // when selection changes in the first combo, if the second one it not empty
            // select the first item
            // this is a kind of a hack because relying only on binding doesn't work (probably for timing reasons)
            if (this.CbFilters.Items.Count > 0)
                this.CbFilters.SelectedIndex = 0;
        }
    }
}
