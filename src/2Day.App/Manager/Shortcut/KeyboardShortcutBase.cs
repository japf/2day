using System;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public abstract class KeyboardShortcutBase
    {
        private readonly Frame rootFrame;
        protected readonly INavigationService navigationService;

        protected KeyboardShortcutBase(Frame rootFrame, INavigationService navigationService)
        {
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            this.rootFrame = rootFrame;
            this.navigationService = navigationService;
        }

        protected bool IsFromTextControl(KeyRoutedEventArgs e)
        {
            return (e.OriginalSource is TextBox) || 
                (e.OriginalSource is RichEditBox) || 
                (e.OriginalSource is PasswordBox) || 
                (e.OriginalSource is AutoSuggestBox);
        }

        protected bool IsModifierKeyDown(VirtualKey key)
        {
            bool isDown = false;
            try
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(key);
                isDown = (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            }
            catch (Exception)
            {
            }

            return isDown;
        }

        protected T GetCurrentPageDataContextAs<T>() where T : INotifyPropertyChanged
        {
            var flyout = ((NavigationService) this.navigationService).PeekFlyout();
            if (flyout != null && flyout.Content != null && ((FrameworkElement)flyout.Content).DataContext is T)
                return (T)((FrameworkElement)flyout.Content).DataContext;

            var content = this.rootFrame.Content as FrameworkElement;
            if (content != null && content.DataContext is T)
                return (T)content.DataContext;

            return default(T);
        }
        
        protected T GetCurrentPageAs<T>() where T : Page
        {
            if (this.rootFrame.Content is T)
            {
                return (T)this.rootFrame.Content;
            }
            else
            {
                var flyout = ((NavigationService)this.navigationService).PeekFlyout();
                if (flyout != null && flyout.Content != null && ((FrameworkElement)flyout.Content) is T)
                    return (T)((FrameworkElement)flyout.Content);
            }
            
            return default(T);
        }

        public abstract bool CanExecute(KeyRoutedEventArgs e);

        public abstract bool Execute(KeyRoutedEventArgs e);
    }
}