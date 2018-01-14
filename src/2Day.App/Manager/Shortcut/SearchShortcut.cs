using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class SearchShortcut : KeyboardShortcutBase
    {
        public SearchShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return (e.Key == VirtualKey.F && this.IsModifierKeyDown(VirtualKey.Control)) && this.GetCurrentPageAs<MainPage>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var page = this.GetCurrentPageAs<MainPage>();
            if (page != null)
            {
                page.FocusSearchBox();
                return true;
            }


            return false;
        }
    }
}