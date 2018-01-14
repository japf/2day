using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class ToastIndicator : UserControl
    {
        private static SolidColorBrush normalBackgroundBrush;
        private static SolidColorBrush normalForegroundBrush;
        private static SolidColorBrush warningBackgroundBrush;
        private static SolidColorBrush warningForegroundBrush;
        
        public bool IsAnimationRunning { get; set; }

        public static readonly DependencyProperty IsAnimatedProperty = DependencyProperty.Register(
            "IsAnimated",
            typeof(bool),
            typeof(ToastIndicator),
            new PropertyMetadata(false, OnIsAnimatedChanged));

        public bool IsAnimated
        {
            get { return (bool)this.GetValue(IsAnimatedProperty); }
            set { this.SetValue(IsAnimatedProperty, value); }
        }

        public string IconCode
        {
            get { return this.icon.Text; }
            set { this.icon.Text = value ?? string.Empty; }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description",
            typeof(string),
            typeof(ToastIndicator),
            new PropertyMetadata(string.Empty, OnDescriptionChanged));

        public string Description
        {
            get { return (string)this.GetValue(DescriptionProperty); }
            set { this.SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty ToastTypeProperty = DependencyProperty.Register(
            "ToastType",
            typeof(ToastType),
            typeof(ToastIndicator),
            new PropertyMetadata(ToastType.Default, OnToastTypeChanged));

        public ToastType ToastType
        {
            get { return (ToastType)this.GetValue(ToastTypeProperty); }
            set { this.SetValue(ToastTypeProperty, value); }
        }

        private Thickness borderThiness;

        public ToastIndicator()
        {
            this.InitializeComponent();
            this.Opacity = 0;

            this.slideDownStoryboard.Completed += (s, e) =>
            {
                this.IsAnimationRunning = false;

                this.Opacity = 0;
                this.MinWidth = 0;

                this.progressRing.IsActive = false;
                this.Description = null;
            };

            this.slideUpStoryboard.Completed += (s, e) =>
            {
                this.IsAnimationRunning = false;
            };

            normalBackgroundBrush = Application.Current.Resources["NavMenuBackgroundBrush"] as SolidColorBrush;
            normalForegroundBrush = Application.Current.Resources["ForegroundBrush"] as SolidColorBrush;

            warningBackgroundBrush = Application.Current.Resources["ApplicationToastWarningBrush"] as SolidColorBrush;
            warningForegroundBrush = Application.Current.Resources["ApplicationWhiteForegroundBrush"] as SolidColorBrush;
        }

        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var indicator = (ToastIndicator) d;
            if (indicator.textBlock != null && !string.IsNullOrEmpty(e.NewValue as string))
            {
                // if message is too long, reduce font size to display whole content
                int length = ((string) e.NewValue).Length;
                indicator.textBlock.FontSize = length > 75 ? 12 : 13;

                if (!indicator.IsAnimated)
                {
                    indicator.progressRing.IsActive = false;
                    indicator.progressRing.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static void OnIsAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var indicator = (ToastIndicator) d;
            if (indicator.IsAnimated)
            {
                indicator.progressRing.IsActive = true;
                indicator.progressRing.Visibility = Visibility.Visible;
                indicator.icon.Visibility = Visibility.Collapsed;
            }
            else
            {
                indicator.progressRing.IsActive = false;
                indicator.progressRing.Visibility = Visibility.Collapsed;
            }
        }

        private static void OnToastTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var indicator = (ToastIndicator) d;

            switch (indicator.ToastType)
            {
                case ToastType.Default:
                case ToastType.Info:
                case ToastType.Search:
                    indicator.mainBorder.Background = normalBackgroundBrush;
                    indicator.mainBorder.BorderThickness = indicator.borderThiness;
                    indicator.Foreground = normalForegroundBrush;

                    break;
                case ToastType.Warning:
                    indicator.mainBorder.Background = warningBackgroundBrush;
                    indicator.mainBorder.BorderThickness = new Thickness();
                    indicator.Foreground = warningForegroundBrush;
                    break;
            }

        }

        public void Show(ToastType type, bool cancel = false)
        {
            this.MinWidth = 300;

            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                this.MaxWidth = double.MaxValue;
                this.borderThiness = new Thickness(0, 1, 0, 0);
            }
            else
            {
                this.MaxWidth = 300;
                this.borderThiness = new Thickness(1, 1, 1, 0);                
            }

            this.ToastType = type;

            this.cancelSymbol.Visibility = cancel ? Visibility.Visible : Visibility.Collapsed;

            if (this.IsAnimated)
            {
                this.progressRing.Visibility = Visibility.Visible;              
                this.progressRing.IsActive = true;
                this.icon.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.progressRing.Visibility = Visibility.Collapsed;
                this.icon.Visibility = Visibility.Visible;
            }

            // at init of after slide down animation opacity is set to 0
            if (this.Opacity == 0)
            {
                this.Opacity = 1;
                this.IsAnimationRunning = false;
                this.slideUpStoryboard.Begin();
            }
        }

        public void Hide()
        {
            if (this.Opacity > 0)
            {
                this.IsAnimationRunning = true;
                this.slideDownStoryboard.Begin();
            }
        }

        public void StopAnimation()
        {
            this.slideDownStoryboard.Stop();
        }
    }
}
