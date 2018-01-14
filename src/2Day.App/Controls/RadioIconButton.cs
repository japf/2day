using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Controls
{
    public class RadioIconButton : RadioButton
    {
        public AppIconType Icon
        {
            get { return (AppIconType) this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", 
            typeof(AppIconType), 
            typeof(RadioIconButton), 
            new PropertyMetadata(AppIconType.CommonAdd));
    }
}
