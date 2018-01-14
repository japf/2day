using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class SaveAndContinueShortcut : KeyboardShortcutBase
    {
        public SaveAndContinueShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return (e.Key == VirtualKey.Enter && this.IsModifierKeyDown(VirtualKey.Control))
                && this.GetCurrentPageDataContextAs<CreateTaskViewModel>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var viewmodel = this.GetCurrentPageDataContextAs<CreateTaskViewModel>();
            if (viewmodel != null && !string.IsNullOrEmpty(viewmodel.Title))
            {
                viewmodel.QuickAddAsync();
                return true;
            }

            return false;
        }
    }
}