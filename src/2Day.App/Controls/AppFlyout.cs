using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    [ContentProperty(Name = "Content")]
    public class AppFlyout : FrameworkElement
    {
        private static readonly Point emptyPoint = new Point();

        private Popup popup;
        private FrameworkElement owner;
        
        public FrameworkElement Content
        {
            get { return (FrameworkElement) this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content),
            typeof(UIElement),
            typeof(AppFlyout),
            new PropertyMetadata(null));

        public static DateTime PopupCloseLastTime { get; set; }

        public void Initialize(UIElement owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            this.owner = (FrameworkElement) owner;
            this.owner.DataContextChanged += (s, e) =>
            {
                // the Content of this flyout is disconnected from the visual tree so we have to "plug" the datacontext manually
                if (this.Content != null)
                    this.Content.DataContext = this.owner.DataContext;
            };

            this.popup = new Popup
            {
                IsLightDismissEnabled = true,
                ChildTransitions = new TransitionCollection()
            };
            this.popup.Closed += OnPopupClosed;
            this.popup.ChildTransitions.Add(new PaneThemeTransition {Edge = EdgeTransitionLocation.Top});

            bool isUsingSmallLayout = ResponsiveHelper.IsUsingSmallLayout();

            var popupChild = new Border
            {
                BorderBrush = this.FindResource<SolidColorBrush>("AppFlyoutBorderBrush"),
                Background = this.FindResource<SolidColorBrush>("AppFlyoutButtonBackgroundBrush"),
                Child = this.Content,
            };

            if (isUsingSmallLayout)
            {
                if (Window.Current != null)
                {
                    Rect bounds = Window.Current.Bounds;
                    popupChild.Width = bounds.Width;
                }
                else
                {
                    popupChild.Width = 320;
                }
                popupChild.BorderThickness = new Thickness(0, 1, 0, 1);
            }
            else
            {
                popupChild.Width = (double) SettingsSizeMode.Small;
                popupChild.BorderThickness = new Thickness(1);
            }

            this.popup.Child = popupChild;

            if (this.Content != null)
                this.Content.Loaded += this.OnContentLoaded;
            
            this.owner.Tapped += (s, e) => this.Show(e.OriginalSource as FrameworkElement);
        }

        private static void OnPopupClosed(object sender, object e)
        {
            PopupCloseLastTime = DateTime.Now;
        }

        private void OnContentLoaded(object sender, object e)
        {
            Page page = TreeHelper.FindVisualAncestor<Page>(this.owner);
            if (page == null || !(this.popup.Child is FrameworkElement))
                return;

            Point relativeToPage = this.popup.TransformToVisual(page).TransformPoint(emptyPoint);
            FrameworkElement popupChild = (FrameworkElement) this.popup.Child;
            double popupWidth = popupChild.ActualWidth;

            bool isUsingSmallLayout = ResponsiveHelper.IsUsingSmallLayout();
            if (isUsingSmallLayout)
            {
                if (Window.Current != null && popupChild.ActualHeight >= 150)
                {
                    // small layout with large popup => center in screen
                    Rect bounds = Window.Current.Bounds;
                    popupChild.Height = bounds.Height;
                    this.popup.HorizontalOffset = 0;
                    this.popup.VerticalOffset = 0;

                    var dateTimePicker = TreeHelper.FindVisualChild<DateTimePicker2>(popupChild);
                    if (dateTimePicker != null)
                    {
                        dateTimePicker.VerticalAlignment = VerticalAlignment.Center;
                    }
                }
            }
            else if (page.Width > 0)
            {
                this.popup.HorizontalOffset = -relativeToPage.X + (page.Width - popupWidth) / 2;
            }

            // check if the popup is not going to be shown out of screen vertically
            if (this.popup.VerticalOffset > 0 && Window.Current != null)
            {
                Rect bounds = Window.Current.Bounds;
                if (this.popup.VerticalOffset + popupChild.ActualHeight > bounds.Height)
                {
                    this.popup.VerticalOffset -= (this.popup.VerticalOffset + popupChild.ActualHeight - bounds.Height);
                }
            }
        }
        
        public void Show(FrameworkElement placementTarget)
        {
            if (placementTarget != null)
            {
                var taskFieldEntry = TreeHelper.FindVisualAncestor<TaskFieldEntry>(placementTarget);
                var button = TreeHelper.FindVisualAncestor<Button>(placementTarget);

                if (button != null && button == this.owner)
                {
                    Point relativeToPage = this.popup.TransformToVisual(button).TransformPoint(emptyPoint);
                    this.popup.VerticalOffset = -relativeToPage.Y + button.ActualHeight - 1;
                }
                else if (taskFieldEntry != null)
                {
                    Point relativeToPage = this.popup.TransformToVisual(taskFieldEntry).TransformPoint(emptyPoint);
                    this.popup.VerticalOffset = -relativeToPage.Y + taskFieldEntry.ActualHeight - 1;
                }
            }

            this.popup.IsOpen = true;
        }

        public void Hide()
        {
            this.popup.IsOpen = false;
        }
    }
}