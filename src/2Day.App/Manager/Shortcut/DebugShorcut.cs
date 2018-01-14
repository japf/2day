using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class DebugShorcut : KeyboardShortcutBase
    {
        public DebugShorcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
#if DEBUG
            return !this.IsFromTextControl(e) && e.Key == VirtualKey.D && this.GetCurrentPageDataContextAs<MainPageViewModel>() != null && !this.IsFromTextControl(e);
#else
            return false;
#endif
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var viewmodel = this.GetCurrentPageDataContextAs<MainPageViewModel>();
            if (viewmodel != null)
            {
                this.navigationService.FlyoutTo(typeof(TaskPage));
                //viewmodel.OpenDebugCommand.Execute(null);
                return true;
            }

            return false;
        }
    }
}