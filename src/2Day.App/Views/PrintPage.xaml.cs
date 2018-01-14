using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class PrintPage : Page
    {
        public PrintPage()
        {
            this.InitializeComponent();
        }

        public PrintPage(double horizontalMargin, double verticalMargin)
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.root.Margin = new Thickness(horizontalMargin, verticalMargin, horizontalMargin, verticalMargin);
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var folderItemViewModel = (FolderItemViewModel)e.Parameter;
            var content = new List<object>();
            foreach (var group in folderItemViewModel.SmartCollection.Items)
            {
                content.Add(group);
                foreach (var task in group)
                {
                    content.Add(task);
                }
            }

            this.DataContext = content;
        }
    }

    public class PrintPageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate GroupTemplate { get; set; }
        
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ITask)
                return this.ItemTemplate;
            
            return this.GroupTemplate;
        }
    }
}
