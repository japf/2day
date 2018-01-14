using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class EmptyListHint : UserControl
    {
        public string Message
        {
            get { return (string) this.GetValue(MessageProperty); }
            set { this.SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", 
            typeof(string), 
            typeof(EmptyListHint), 
            new PropertyMetadata(string.Empty));

        public string Hint
        {
            get { return (string) this.GetValue(HintProperty); }
            set { this.SetValue(HintProperty, value); }
        }

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
            "Hint",
            typeof(string),
            typeof(EmptyListHint),
            new PropertyMetadata(string.Empty));

        public ICommand Command
        {
            get { return (ICommand) this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(EmptyListHint),
            new PropertyMetadata(null));

        public EmptyListHint()
        {
            this.InitializeComponent();
        }
    }
}
