using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class PrintShortcut : KeyboardShortcutBase
    {
        public PrintShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return (e.Key == VirtualKey.P && this.IsModifierKeyDown(VirtualKey.Control)) && this.GetCurrentPageDataContextAs<MainPageViewModel>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var viewmodel = this.GetCurrentPageDataContextAs<MainPageViewModel>();
            if (viewmodel != null)
            {
                viewmodel.PrintCommand.Execute(null);
                return true;
            }

            return false;
        }
    }
}