using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Chartreuse.Today.App.Tools.UI;

namespace Chartreuse.Today.App.Controls
{
    public class SwipableListViewItem : Control
    {
        private static SwipableListViewItem lastActionListItemAnimated;

        private Panel content;
        private Control leftContent;
        private Control rightContent;
        private ScrollViewer scrollviewer;

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", 
            typeof(object), 
            typeof(SwipableListViewItem), 
            new PropertyMetadata(null));
        
        public static readonly DependencyProperty LeftContentProperty = DependencyProperty.Register(
            "LeftContent", 
            typeof(object), 
            typeof(SwipableListViewItem), 
            new PropertyMetadata(null));
        
        public static readonly DependencyProperty LeftBackgroundProperty = DependencyProperty.Register(
            "LeftBackground", 
            typeof(Brush), 
            typeof(SwipableListViewItem), 
            new PropertyMetadata(null));

        public static readonly DependencyProperty LeftCommandProperty = DependencyProperty.Register(
            "LeftCommand", 
            typeof(ICommand), 
            typeof(SwipableListViewItem),
            new PropertyMetadata(null));
        
        public static readonly DependencyProperty RightCommandProperty = DependencyProperty.Register(
            "RightCommand", 
            typeof(ICommand), 
            typeof(SwipableListViewItem), 
            new PropertyMetadata(null));

        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
            "RightContent", 
            typeof(object), 
            typeof(SwipableListViewItem), 
            new PropertyMetadata(null));

        public static readonly DependencyProperty RightBackgroundProperty = DependencyProperty.Register(
            "RightBackground", 
            typeof(Brush), 
            typeof(SwipableListViewItem), 
            new PropertyMetadata(null));

        public object Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }
        
        public object LeftContent
        {
            get { return this.GetValue(LeftContentProperty); }
            set { this.SetValue(LeftContentProperty, value); }
        }
        
        public Brush LeftBackground
        {
            get { return (Brush)this.GetValue(LeftBackgroundProperty); }
            set { this.SetValue(LeftBackgroundProperty, value); }
        }

        public ICommand LeftCommand
        {
            get { return (ICommand)this.GetValue(LeftCommandProperty); }
            set { this.SetValue(LeftCommandProperty, value); }
        }

        public object RightContent
        {
            get { return this.GetValue(RightContentProperty); }
            set { this.SetValue(RightContentProperty, value); }
        }
        
        public Brush RightBackground
        {
            get { return (Brush)this.GetValue(RightBackgroundProperty); }
            set { this.SetValue(RightBackgroundProperty, value); }
        }

        public ICommand RightCommand
        {
            get { return (ICommand)this.GetValue(RightCommandProperty); }
            set { this.SetValue(RightCommandProperty, value); }
        }

        public SwipableListViewItem()
        {
            this.DefaultStyleKey = typeof(SwipableListViewItem);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.content = (Panel)this.GetTemplateChild("PART_Content");

            this.leftContent = (Control)this.GetTemplateChild("PART_LeftContent");
            this.rightContent = (Control)this.GetTemplateChild("PART_RightContent");

            this.content.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.System;
            this.content.ManipulationDelta += this.OnContentManipulationDelta;
            this.content.ManipulationCompleted += this.OnContentManipulationCompleted;
        }

        public async void Reset()
        {
            if (this.leftContent.Visibility == Visibility.Visible || this.rightContent.Visibility == Visibility.Visible)
            {
                await this.FadeOutControl(0);
                this.leftContent.Visibility = Visibility.Collapsed;
                this.rightContent.Visibility = Visibility.Collapsed;
            }
        }

        private void OnContentManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (lastActionListItemAnimated != null)
            {
                lastActionListItemAnimated.Reset();
                lastActionListItemAnimated = null;
            }

            e.Handled = false;
            var target = e.Cumulative.Translation.X;
            if (target < 0)
            {
                if (this.rightContent.ActualWidth > 0 && Math.Abs(target) > this.rightContent.ActualWidth)
                {
                    target = this.rightContent.ActualWidth * -1;
                }
            }

            this.leftContent.Visibility = target > 0 ? Visibility.Visible : Visibility.Collapsed;
            this.rightContent.Visibility = target < 0 ? Visibility.Visible : Visibility.Collapsed;

            if (this.content.RenderTransform is CompositeTransform)
                ((CompositeTransform) this.content.RenderTransform).TranslateX = target;
        }

        private async void OnContentManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if ((e.Cumulative.Translation.X > 0 && e.Cumulative.Translation.X >= 150) ||
                (e.Cumulative.Translation.X < 0 && Math.Abs(e.Cumulative.Translation.X) >= this.rightContent.ActualWidth))
            {
                if (e.Cumulative.Translation.X > 0)
                {
                    await this.FadeOutControlDirection(AnimationDirection.Right);

                    if (this.LeftCommand != null && this.LeftCommand.CanExecute(null))
                        this.LeftCommand.Execute(this.DataContext);

                    await this.FadeOutControl(0);

                }
                else if (e.Cumulative.Translation.X < 0)
                {
                    lastActionListItemAnimated = this;
                    if (this.scrollviewer == null)
                    {
                        this.scrollviewer = TreeHelper.FindVisualAncestor<ScrollViewer>(this);
                        this.scrollviewer.ViewChanging += this.OnScrollViewerViewChanging;
                    }                   
                }
            }
            else
            {
                await this.FadeOutControl(0d);
                this.leftContent.Visibility = Visibility.Collapsed;
                this.rightContent.Visibility = Visibility.Collapsed;
            }
        }

        private void OnScrollViewerViewChanging(object sender, object e)
        {
            this.scrollviewer.ViewChanging -= this.OnScrollViewerViewChanging;
            this.Reset();
        }

        private Task FadeOutControlDirection(AnimationDirection direction)
        {
            double newLeft = direction == AnimationDirection.Left ? this.ActualWidth * -1d : this.ActualWidth;
            return this.FadeOutControl(newLeft);
        }

        private Task FadeOutControl(double target)
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(200);

            DoubleAnimationUsingKeyFrames doubleAnimationKeyFrames = new DoubleAnimationUsingKeyFrames();

            if (this.content.RenderTransform is CompositeTransform)
                doubleAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = ((CompositeTransform) this.content.RenderTransform).TranslateX });

            doubleAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = target, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } });

            Storyboard.SetTarget(doubleAnimationKeyFrames, this.content);
            Storyboard.SetTargetProperty(doubleAnimationKeyFrames, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            Storyboard storyBoard = new Storyboard { Duration = duration };

            storyBoard.Children.Add(doubleAnimationKeyFrames);
            storyBoard.Completed += (s, e) => source.SetResult(true);
            storyBoard.Begin();

            lastActionListItemAnimated = null;

            return source.Task;
        }

    }
}
