using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class EditBox : UserControl, ICustomDialogContent
    {
        public string Text
        {
            get { return this.textbox.Text; }
        }

        public EditBox(string placeholder, string text)
        {
            this.InitializeComponent();

            this.textbox.PlaceholderText = placeholder;
            if (!string.IsNullOrEmpty(text))
                this.textbox.Text = text;

            this.Loaded += (s, e) => this.textbox.Focus(FocusState.Programmatic);
            this.textbox.KeyUp += this.OnTextBoxKeyUp;
        }

        private void OnTextBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                this.RequestClose.Raise(this);
        }

        private void OnButtonOkContextClick(object sender, RoutedEventArgs e)
        {
            this.RequestClose.Raise(this);
        }

        public void OnDismissed()
        {
        }

        public event EventHandler RequestClose;
    }
}
