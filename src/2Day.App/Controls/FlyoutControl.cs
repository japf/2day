using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.Core.Shared.Annotations;

namespace Chartreuse.Today.App.Controls
{
    public class FlyoutControl : Control
    {
        private double topMargin = 30;

        // This is the container that will hold our custom content.
        private Popup settingsPopup;
        private Action<FlyoutControl> removeHandler;
        private bool staysOpen;

        public object Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", 
            typeof(object), 
            typeof(FlyoutControl), 
            null);

        public SettingsSizeMode Size { get; set; }

        public bool DockOnLeft { get; set; }

        public bool StaysOpen
        {
            get { return this.staysOpen; }
            set
            {
                if (this.settingsPopup != null)
                {
                    this.settingsPopup.IsLightDismissEnabled = !value;
                }
                this.staysOpen = value;
            }
        }

        public async Task Show([NotNull] Action<FlyoutControl> removeHandler)
        {
            if (removeHandler == null)  
                throw new ArgumentNullException(nameof(removeHandler));

            this.removeHandler = removeHandler;

            Rect windowBounds = GetWindowVisibleBounds();

            // Create a Popup window which will contain our flyout.
            this.settingsPopup = new Popup();
            this.settingsPopup.Closed += this.OnPopupClosed;

            Window.Current.Activated += this.OnWindowActivated;
            Window.Current.SizeChanged += this.OnWindowSizeChanged;

            double flyoutWidth = (double)this.Size;

            this.settingsPopup.IsLightDismissEnabled = !this.staysOpen;
            this.settingsPopup.Width = flyoutWidth;
            this.settingsPopup.Height = windowBounds.Height;

            // We have some weird stuff when 2Day is running with continuum, basically we need a higher top margin because it looks like the 
            // ApplicationView.GetForCurrentView().VisibleBounds include some viewport's height that is not included for standard mobile/tablet/desktop
            if (!ResponsiveHelper.IsInContinuum())
            {
                this.topMargin = 30;
                this.settingsPopup.Height -= this.topMargin;
            }
            else
            {
                this.topMargin = 30 + 24; // 24 is the additional top margin we're adding for continuum
                this.settingsPopup.Height -= 30;
            }

            // Add the proper animation for the panel.
            this.settingsPopup.ChildTransitions = new TransitionCollection
            {
                new PaneThemeTransition
                {
                    Edge = this.DockOnLeft ? EdgeTransitionLocation.Left : EdgeTransitionLocation.Right
                }
            };

            // Create a SettingsFlyout the same dimensions as the Popup.
            var pane = this.Content as FrameworkElement;
            pane.Width = flyoutWidth;
            pane.Height = this.settingsPopup.Height;

            if (pane.Parent is Popup)
            {
                ((Popup) pane.Parent).Child = null;
                // delay to make sure the pane gets disconnected from its parent
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            // Place the SettingsFlyout inside our Popup window.
            this.settingsPopup.Child = pane;

            // Let's define the location of our Popup.
            this.settingsPopup.SetValue(Canvas.LeftProperty, this.DockOnLeft ? 0 : (windowBounds.Width - flyoutWidth));
            this.settingsPopup.SetValue(Canvas.TopProperty, this.topMargin);
            this.settingsPopup.IsOpen = true;
        }

        private static Rect GetWindowVisibleBounds()
        {
            try
            {
                Rect windowBounds = Window.Current.Bounds;
                var currentView = ApplicationView.GetForCurrentView();
                if (currentView != null)
                    windowBounds = currentView.VisibleBounds;
                return windowBounds;
            }
            catch
            {
                return Window.Current.Bounds;
            }            
        }

        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (Window.Current != null)
            {
                Rect windowBounds = Window.Current.Bounds;
                if (this.settingsPopup != null)
                {
                    this.settingsPopup.Height = windowBounds.Height - this.topMargin;
                    if (this.settingsPopup.Child != null)
                        ((FrameworkElement) this.settingsPopup.Child).Height = windowBounds.Height - this.topMargin;
                    this.settingsPopup.SetValue(Canvas.LeftProperty, this.DockOnLeft ? 0 : (windowBounds.Width - this.settingsPopup.Width));
                }
            }
        }

        public void Close()
        {
            this.settingsPopup.IsOpen = false;
        }
        
        /// <summary>
        /// We use the window's activated event to force closing the Popup since a user maybe interacted with
        /// something that didn't normally trigger an obvious dismiss.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated && !this.staysOpen && this.settingsPopup != null)
            {
                this.settingsPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// When the Popup closes we no longer need to monitor activation changes.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= this.OnWindowActivated;
            Window.Current.SizeChanged -= this.OnWindowSizeChanged;

            this.settingsPopup.Closed -= this.OnPopupClosed;
            this.settingsPopup.ClearValue(FlyoutControl.ContentProperty);
            this.settingsPopup.Child = null;

            if (this.Content is FrameworkElement)
            {
                FrameworkElement element = (FrameworkElement)this.Content;
                if (element.DataContext is IDisposable)
                    ((IDisposable)element.DataContext).Dispose();
            }

            this.removeHandler(this);
        }        
    }
}
