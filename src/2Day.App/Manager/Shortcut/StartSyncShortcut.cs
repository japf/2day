using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class StartSyncShortcut : KeyboardShortcutBase
    {
        public StartSyncShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return e.Key == VirtualKey.U
                   && this.IsModifierKeyDown(VirtualKey.Control)
                   && this.GetCurrentPageDataContextAs<MainPageViewModel>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            MainPageViewModel mainPageViewModel = this.GetCurrentPageDataContextAs<MainPageViewModel>();
            if (mainPageViewModel != null)
            {
                mainPageViewModel.SyncCommand.Execute(null);
                return true;
            }

            return false;
        }
    }
}