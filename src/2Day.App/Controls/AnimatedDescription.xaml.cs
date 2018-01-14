using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class AnimatedDescription : UserControl
    {
        public string Text
        {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", 
            typeof(string),
            typeof(AnimatedDescription), 
            new PropertyMetadata(string.Empty));

        public string Description
        {
            get { return (string) this.GetValue(DescriptionProperty); }
            set { this.SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", 
            typeof(string), 
            typeof(AnimatedDescription), 
            new PropertyMetadata(null));

        public AnimatedDescription()
        {
            this.InitializeComponent();
        }
    }
}
