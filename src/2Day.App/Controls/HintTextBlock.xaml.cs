using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class HintTextBlock : UserControl
    {
        public string Text
        {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", 
            typeof(string), 
            typeof(HintTextBlock), 
            new PropertyMetadata(string.Empty));

        public Visibility AnimatedVisibility
        {
            get { return (Visibility) this.GetValue(AnimatedVisibilityProperty); }
            set { this.SetValue(AnimatedVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AnimatedVisibilityProperty = DependencyProperty.Register(
            "AnimatedVisibility", 
            typeof(Visibility), 
            typeof(HintTextBlock), 
            new PropertyMetadata(Visibility.Visible, OnAnimatedVisibilityChanged));
        
        public HintTextBlock()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                if (this.AnimatedVisibility == Visibility.Visible)
                    this.Visibility = Visibility.Visible;
                else
                    this.Visibility = Visibility.Collapsed;
            };
        }

        private static void OnAnimatedVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HintTextBlock)d).OnAnimatedVisibilityChanged(e);
        }

        private async Task OnAnimatedVisibilityChanged(DependencyPropertyChangedEventArgs e)
        {
            if (((Visibility) e.NewValue) == Visibility.Visible)
            {
                this.Visibility = Visibility.Visible;
                this.Opacity = 0;

                this.SlideHorizontalAsync(TimeSpan.FromMilliseconds(600), -10, 0);
                this.AnimateOpacity(1, TimeSpan.FromMilliseconds(600));
            }
            else
            {
                this.AnimateOpacity(0, TimeSpan.FromMilliseconds(600));
                await this.SlideHorizontalAsync(TimeSpan.FromMilliseconds(600), 0, 10);

                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}
