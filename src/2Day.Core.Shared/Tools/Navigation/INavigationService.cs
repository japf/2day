using System;

namespace Chartreuse.Today.Core.Shared.Tools.Navigation
{
    public interface INavigationService
    {
        event EventHandler FlyoutClosing;

        bool HasFlyoutOpened { get; }

        bool CanGoBack { get; }

        void Navigate(Type type);
        void Navigate(Type type, object parameter);

        void GoBack();

        void FlyoutTo(Type type, object parameter = null);

        void OpenSettings();

        void ClearBackStack();

        void CloseFlyouts();
    }
}
