using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class AutoFocusTextBox
    {
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AutoFocusTextBox),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Loaded += OnTextBoxLoaded;
        }

        private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox) sender;
            textbox.Focus(FocusState.Programmatic);
        }        
    }
}
