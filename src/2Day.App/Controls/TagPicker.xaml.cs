using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.ViewModel;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class TagPicker : UserControl
    {
        public IEnumerable<ItemCountViewModel> Tags
        {
            get { return (IEnumerable<ItemCountViewModel>) this.GetValue(TagsProperty); }
            set { this.SetValue(TagsProperty, value); }
        }

        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
            "Tags", 
            typeof(IEnumerable<ItemCountViewModel>), 
            typeof(TagPicker), 
            new PropertyMetadata(null));

        public TagPicker()
        {
            this.InitializeComponent();
        }

        private void OnButtonChooseTagClick(object sender, RoutedEventArgs e)
        {
            // try to find parent popup
            var popup = TreeHelper.FindParent<Popup>(this);
            if (popup != null)
                popup.IsOpen = false;
        }
    }
}
