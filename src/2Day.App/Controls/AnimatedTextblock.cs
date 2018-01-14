using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Controls
{
    public class AnimatedTextBlock : Control
    {
        private readonly ISubject<int> subject;

        private readonly Storyboard storyboard1;
        private readonly Storyboard storyboard2;

        private Border border1;
        private Border border2;

        private Border animatedBorder1;
        private Border animatedBorder2;

        private bool isTemplateLoaded;
        private Action templateLoadedAction;

        public int Count
        {
            get { return (int)this.GetValue(CountProperty); }
            set { this.SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count",
            typeof(int),
            typeof(AnimatedTextBlock),
            new PropertyMetadata(0, new PropertyChangedCallback(OnCountChanged)));

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(AnimatedTextBlock),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        public AnimatedTextBlock()
        {
            this.DefaultStyleKey = typeof(AnimatedTextBlock);

            this.subject = new Subject<int>();
            this.storyboard1 = new Storyboard { Duration = new Duration(TimeSpan.FromSeconds(0.1)) };
            this.storyboard2 = new Storyboard { Duration = new Duration(TimeSpan.FromSeconds(0.1)) };

            this.UpdateStoryboard(this.storyboard1, -90.0, 0.0);
            this.UpdateStoryboard(this.storyboard2, 0.0, 90.0);

            IObservable<EventPattern<object>> second = Observable
                .FromEventPattern<EventHandler<object>, object>(ehea => ehea.Invoke, eh => this.storyboard1.Completed += eh, eh => this.storyboard1.Completed -= eh)
                .StartWith<EventPattern<object>>(new[] { new EventPattern<object>(this.storyboard1, EventArgs.Empty) });

            this.subject
                .Zip(second, (a0, a1) => a0)
                .Subscribe<int>(this.OnNext, this.OnCompleted);
        }

        private void OnNext(int value)
        {
            this.OnNext(value.ToString(CultureInfo.InvariantCulture));
        }

        private void OnNext(string value, bool animate = true)
        {
            if (!this.isTemplateLoaded)
            {
                this.templateLoadedAction = () => this.OnNext(value);
                return;
            }

            if (animate)
            {
                if (this.animatedBorder2 != null)
                {
                    this.animatedBorder2.Visibility = Visibility.Collapsed;
                    this.animatedBorder1 = this.border1.Visibility != Visibility.Visible ? this.border1 : this.border2;
                }
                else
                {
                    this.animatedBorder1 = this.border1.Visibility != Visibility.Visible ? this.border1 : this.border2;
                }

                this.animatedBorder2 = this.animatedBorder1 != this.border1 ? this.border1 : this.border2;

                this.storyboard1.Stop();
                this.storyboard2.Stop();

                Storyboard.SetTarget(this.storyboard1.Children[0], (DependencyObject)this.animatedBorder1.Projection);
                Storyboard.SetTarget(this.storyboard2.Children[0], (DependencyObject)this.animatedBorder2.Projection);

                ((TextBlock)this.animatedBorder1.Child).Text = value;

                this.animatedBorder1.Visibility = Visibility.Visible;
                this.animatedBorder2.Visibility = Visibility.Visible;

                this.storyboard1.Begin();
                this.storyboard2.Begin();
            }
            else
            {
                this.animatedBorder1 = this.border1.Visibility != Visibility.Visible ? this.border1 : this.border2;
                this.animatedBorder2 = this.animatedBorder1 != this.border1 ? this.border1 : this.border2;
                ((TextBlock)this.animatedBorder1.Child).Text = value;
            }
        }

        private void OnCompleted()
        {
            if (this.animatedBorder2 != null)
            {
                this.animatedBorder2.Visibility = Visibility.Visible;
            }
        }

        private void UpdateStoryboard(Storyboard s, double start, double end)
        {
            DoubleAnimation animation2 = new DoubleAnimation
            {
                Duration = s.Duration,
                From = start,
                To = end
            };

            DoubleAnimation animation = animation2;

            s.Children.Add(animation);

            Storyboard.SetTargetProperty(animation, "RotationX");
        }

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedTextBlock animatedTextblock = (AnimatedTextBlock)d;
            animatedTextblock.OnCountChanged((int)e.NewValue, (int)e.OldValue);
        }

        private void OnCountChanged(int newValue, int oldValue)
        {
            Observable
                .Range(oldValue + 1, newValue - oldValue)
                .TakeLast<int>(5)
                .Subscribe<int>(new Action<int>(this.subject.OnNext));
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedTextBlock animatedTextblock = (AnimatedTextBlock)d;
            animatedTextblock.OnTextChanged((string)e.NewValue, (string)e.OldValue);
        }

        private static bool hasReportedException = false;

        private void OnTextChanged(string newValue, string oldValue)
        {
            try
            {
                this.OnNext(oldValue);
                this.OnNext(newValue);
            }
            catch (Exception e)
            {
                if (!hasReportedException)
                {
                    bool hasThreadAccess = this.Dispatcher.HasThreadAccess;
                    bool syncInProgress = Ioc.Resolve<ISynchronizationManager>().IsSyncRunning;

                    TrackingManagerHelper.Exception(e, string.Format("AnimatedTextBlock changes from {0} to {1} thread: {2} sync: {3}", oldValue, newValue, hasThreadAccess, syncInProgress));

                    hasReportedException = true;
                }
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.border1 = this.GetTemplateChild("PART_A") as Border;
            if (this.border1 == null || this.border1.Child as TextBlock == null)
                throw new NotSupportedException("Tempate must contain a Border named PART_A with a TextBlock inside");

            this.border2 = this.GetTemplateChild("PART_B") as Border;
            if (this.border2 == null)
                throw new NotSupportedException("Tempate must contain a Border named PART_A");

            this.isTemplateLoaded = true;

            if (this.templateLoadedAction != null)
            {
                this.templateLoadedAction();
                this.templateLoadedAction = null;
            }
        }
    }
}
