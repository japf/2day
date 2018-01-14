using System;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestNavigationService : INavigationService
    {
        public void GoBack(bool openSettingsAfter = true)
        {
        }

        public event EventHandler FlyoutClosing;

        public bool HasFlyoutOpened { get; }
        public bool CanGoBack { get; }

        public void Navigate(Type type)
        {
        }

        public void Navigate(Type type, object parameter)
        {
        }

        public void GoBack()
        {
        }

        public void FlyoutTo(Type type, object parameter = null)
        {
        }
        
        public void OpenSettings()
        {
        }

        public void ClearBackStack()
        {
        }

        public void CloseFlyouts()
        {
        }        
    }
}