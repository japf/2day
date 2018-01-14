using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.App.Tools
{
    public class NavigationService : INavigationService
    {
        private readonly Frame mainFrame;
        private readonly IPlatformService platformService;
        private readonly Stack<FlyoutControl> flyouts;
        private readonly Dictionary<Type, object> flyoutsCache;

        public event EventHandler FlyoutClosing;

        public bool CanGoBack
        {
            get { return this.mainFrame.CanGoBack; }
        }

        public bool HasFlyoutOpened
        {
            get { return this.flyouts.Count > 0; }
        }

        public IEnumerable<FlyoutControl> Flyouts
        {
            get { return this.flyouts; }
        }

        public FlyoutControl PeekFlyout()
        {
            if (this.HasFlyoutOpened)
                return this.flyouts.Peek();

            return null;
        }

        protected Frame MainFrame
        {
            get { return this.mainFrame; }
        }

        public NavigationService(Frame mainFrame, IPlatformService platformService = null)
        {
            if (mainFrame == null)
                throw new ArgumentNullException(nameof(mainFrame));

            this.mainFrame = mainFrame;
            this.platformService = platformService;

            this.flyouts = new Stack<FlyoutControl>();
            this.flyoutsCache = new Dictionary<Type, object>();
        }

        public void ClearBackStack()
        {
            this.mainFrame.BackStack.Clear();
        }

        public void Navigate(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.mainFrame.Navigate(type);
        }

        public void Navigate(Type type, object parameter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.mainFrame.Navigate(type, parameter);
        }

        public void FlyoutTo(Type type, object parameter = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!this.IsFlyoutAllowed())
            {
                this.Navigate(type, parameter);
            }
            else
            {
                var currentFlyoutControl = this.PeekFlyout();
                if (currentFlyoutControl != null && currentFlyoutControl.Content.GetType() == type)
                {
                    // if this flyout is currently opened, signal new parameter
                    if (currentFlyoutControl.Content is ISettingsPage)
                        ((ISettingsPage) currentFlyoutControl.Content).OnNavigatedTo(parameter);
                    return;
                }

                SettingsSizeMode size = SettingsSizeMode.Small;

                object content = null;
                if (type == typeof(TaskPage))
                {
                    // only cache TaskPage because of side effects with other pages +
                    // for memory usage (TaskPage is the only one used many times during the lifetime 
                    // of the app)
                    if (!this.flyoutsCache.ContainsKey(type))
                        this.flyoutsCache.Add(type, Activator.CreateInstance(type));
                    content = this.flyoutsCache[type];
                }
                else
                {
                    content = Activator.CreateInstance(type);
                }
                if (content is Page)
                {
                    Page page = (Page) content;

                    if (page is ISettingsPage)
                    {
                        ISettingsPage settingsPage = (ISettingsPage) content;

                        size = settingsPage.Size;
                        settingsPage.OnNavigatedTo(parameter);
                    }
                    else
                    {
                        size = SettingsSizeMode.Small;
                    }
                }

                var flyoutControl = new FlyoutControl {Content = content, Size = size, StaysOpen = true};

                flyoutControl.Show(this.OnFlyoutClosed);

                this.flyouts.Push(flyoutControl);
            }
        }

        private void OnFlyoutClosed(FlyoutControl flyoutControl)
        {
            this.FlyoutClosing.Raise(EventArgs.Empty);

            if (this.flyouts.Count > 0 && this.flyouts.Peek() == flyoutControl)
            {
                this.flyouts.Pop();
                while (this.flyouts.Count > 0)
                {
                    FlyoutControl childControl = this.flyouts.Pop();
                    childControl.Close();
                }
            }
        }

        public void GoBack()
        {
            if (this.flyouts.Count == 0)
            {
                // no flyout => normal go back
                if (this.mainFrame.CanGoBack)
                {
                    if (this.mainFrame.Content is FrameworkElement)
                    {
                        var datacontext = ((FrameworkElement) this.mainFrame.Content).DataContext as IDisposable;
                        if (datacontext != null)
                            datacontext.Dispose();
                    }

                    this.mainFrame.GoBack();
                }
                else
                {
                    // platform service can be null if we instantiante the navigation service manually
                    // during startup to report a crash
                    if (this.platformService != null)
                        this.platformService.ExitAppAsync();
                }
            }
            else
            {
                FlyoutControl flyoutControl = this.flyouts.Pop();
                if (flyoutControl.DataContext is IDisposable)
                    ((IDisposable)flyoutControl.DataContext).Dispose();
                flyoutControl.Close();
            }
        }

        public void CloseFlyouts()
        {
            for (int i = 0; i < this.flyouts.Count; i++)
            {
                bool backExecuted = false;
                var flyout = this.flyouts.Peek();
                var control = flyout.Content as Control;
                if (control != null)
                {
                    var viewmodel = control.DataContext as PageViewModelBase;
                    if (viewmodel != null)
                    {
                        viewmodel.GoBackCommand.Execute(null);
                        backExecuted = true;
                    }
                }

                if (!backExecuted)
                    this.GoBack();
            }
        }

        public void OpenSettings()
        {
            this.FlyoutTo(typeof(SettingsPage));
        }

        private bool IsFlyoutAllowed()
        {
            return this.MainFrame.RenderSize.Width > ResponsiveHelper.MinWidth;
        }
    }
}