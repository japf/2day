using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;

namespace Chartreuse.Today.App.Tools.UI
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is SeparatorItemViewModel)
                return this.SeparatorTemplate;
            else
                return this.ItemTemplate;
        }
    }
}
