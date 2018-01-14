using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class AnimationHelper
    {
        public static void AnimateOpacity(this FrameworkElement element, double to, TimeSpan duration, Action completed = null)
        {
            var sb = new Storyboard();
            sb.Children.Add(new DoubleAnimation { To = to, Duration = duration });

            Storyboard.SetTarget(sb, element);
            Storyboard.SetTargetProperty(sb, "Opacity");

            if (completed != null)
                sb.Completed += (sender, args) => completed();

            sb.Begin();
        }

        public static Task SlideUpAsync(this FrameworkElement element, TimeSpan duration, double height)
        {
            return SlideY(element, duration, height, 0);
        }

        public static Task SlideHorizontalAsync(this FrameworkElement element, TimeSpan duration, double startX, double endX)
        {
            return SlideX(element, duration, startX, endX);
        }

        public static Task SlideDownAsync(this FrameworkElement element, TimeSpan duration, double height)
        {
            return SlideY(element, duration, 0, height);
        }

        public static void SlideFromTop(this FrameworkElement element, TimeSpan duration, bool slideIn, Action completed = null)
        {
            var transform = element.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                element.RenderTransform = transform;
            }

            if (element.DesiredSize.Height <= 0)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            double from = -element.DesiredSize.Height;
            double to = 0;

            if (!slideIn)
            {
                to = from;
                from = 0;
            }
            transform.Y = from;

            var sb = new Storyboard();
            sb.Children.Add(new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = new QuarticEase()
            });

            Storyboard.SetTarget(sb, element);
            Storyboard.SetTargetProperty(sb, "(UIElement.RenderTransform).(TranslateTransform.Y)");

            if (completed != null)
                sb.Completed += (sender, args) => completed();

            sb.Begin();
        }

        private static async Task SlideY(FrameworkElement element, TimeSpan duration, double from, double to)
        {
            await Slide(element, duration, from, to, "TranslateY");
        }

        private static async Task SlideX(FrameworkElement element, TimeSpan duration, double from, double to)
        {
            await Slide(element, duration, from, to, "TranslateX");
        }

        private static Task Slide(FrameworkElement element, TimeSpan duration, double from, double to, string translate)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (!(element.RenderTransform is CompositeTransform))
                element.RenderTransform = new CompositeTransform();

            var sb = new Storyboard();

            //var doubleAnimation = new DoubleAnimation {To = 1.0, Duration = duration};
            //sb.Children.Add(doubleAnimation);
            //Storyboard.SetTargetProperty(doubleAnimation, "Opacity");

            var translateAnimation = new DoubleAnimation {From = from, To = to, Duration = duration, EasingFunction = new CircleEase {EasingMode = EasingMode.EaseOut}};
            sb.Children.Add(translateAnimation);
            Storyboard.SetTargetProperty(translateAnimation, $"(UIElement.RenderTransform).(CompositeTransform.{translate})");

            Storyboard.SetTarget(sb, element);

            sb.Begin();
            sb.Completed += (s, e) => tcs.SetResult(true);

            return tcs.Task;
        }
    }
}
