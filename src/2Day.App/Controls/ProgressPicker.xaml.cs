using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools.UI;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class ProgressPicker : UserControl
    {
        public double? Progress
        {
            get { return (double?) this.GetValue(ProgressProperty); }
            set { this.SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            "Progress", 
            typeof(object), 
            typeof(ProgressPicker), 
            new PropertyMetadata(null, OnPropertyChanged));

        public ProgressPicker()
        {
            this.InitializeComponent();

            this.buttonNone.IsChecked = true;

            this.tbCustomProgress.GotFocus += (s, e) => this.tbCustomProgress.SelectAll();
            this.tbCustomProgress.KeyUp += this.OnTextBoxCustomProgressKeyUp;
        }

        private void OnTextBoxCustomProgressKeyUp(object s, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Flyout flyout = FlyoutBase.GetAttachedFlyout(this.buttonCustom) as Flyout;
                if (flyout != null)                
                    flyout.Hide();

                var popup = this.FindParent<Popup>();
                if (popup != null)
                    popup.IsOpen = false;

                // hack to hide the virtual keyboard
                this.Dispatcher.RunIdleAsync(c => this.buttonHidden.Focus(FocusState.Programmatic));
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProgressPicker)d).OnPropertyChanged(e);
        }

        private void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.buttonNone.IsChecked = false;
            this.button0.IsChecked = false;
            this.button25.IsChecked = false;
            this.button50.IsChecked = false;
            this.button75.IsChecked = false;
            this.button100.IsChecked = false;

            if (this.Progress == null)
                this.buttonNone.IsChecked = true;
            else if (this.Progress == 0.00)
                this.button0.IsChecked = true;
            else if (this.Progress == 0.25)
                this.button25.IsChecked = true;
            else if (this.Progress == 0.50)
                this.button50.IsChecked = true;
            else if (this.Progress == 0.75)
                this.button75.IsChecked = true;
            else if (this.Progress == 1.00)
                this.button100.IsChecked = true;
        }

        private void OnButtonProgressClick(object sender, RoutedEventArgs e)
        {
            if (sender == this.buttonNone)
                this.Progress = null;
            else if (sender == this.button0)
                this.Progress = 0.0;
            else if (sender == this.button25)
                this.Progress = 0.25;
            else if (sender == this.button50)
                this.Progress = 0.50;
            else if (sender == this.button75)
                this.Progress = 0.75;
            else if (sender == this.button100)
                this.Progress = 1.00;

            var popup = this.FindParent<Popup>();
            if (popup != null)
                popup.IsOpen = false;
        }
    }
}
