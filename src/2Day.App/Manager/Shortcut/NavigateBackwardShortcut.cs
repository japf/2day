using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class NavigateBackwardShortcut : KeyboardShortcutBase
    {
        public NavigateBackwardShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            // no backward navigation with AutoSuggestBox because it's used to close the dropdown suggestion
            return ((e.Key == VirtualKey.Back && !this.IsFromTextControl(e)) || (e.Key == VirtualKey.Escape && TreeHelper.FindVisualAncestor<AutoSuggestBox>(e.OriginalSource as UIElement) == null))
                && this.GetCurrentPageAs<MainPage>() == null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            PageViewModelBase pageViewModel = this.GetCurrentPageDataContextAs<PageViewModelBase>();

            if (pageViewModel != null)
            {
                pageViewModel.GoBackCommand.Execute(null);
                return true;
            }
            else if (this.navigationService.CanGoBack)
            {
                this.navigationService.GoBack();
                return true;
            }

            return false;
        }
    }
}