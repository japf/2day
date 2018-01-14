using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Controls
{
    public class ResponsivePivotItem : ContentControl
    {
        public string Header
        {
            get { return (string) this.GetValue(HeaderProperty); }
            set { this.SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", 
            typeof(string), 
            typeof(ResponsivePivotItem), 
            new PropertyMetadata(string.Empty));

        public bool ShowSeparator
        {
            get { return (bool) this.GetValue(ShowSeparatorProperty); }
            set { this.SetValue(ShowSeparatorProperty, value); }
        }

        public static readonly DependencyProperty ShowSeparatorProperty = DependencyProperty.Register(
            "ShowSeparator", 
            typeof(bool), 
            typeof(ResponsivePivotItem), 
            new PropertyMetadata(true));
    }
}
