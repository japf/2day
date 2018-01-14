using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Chartreuse.Today.App.Controls
{
    [DebuggerDisplay("Value: {Value} Mode: {Mode}")]
    public class ClockButton : ToggleButton
    {
        private readonly IClock owner;
        private readonly double centerX;
        private readonly double centerY;
        private bool isDragging;

        public bool IsInner { get; }

        public int Value { get; }

        public ClockItemMember Mode { get; }

        public double TextOpacity
        {
            get { return (double) this.GetValue(TextOpacityProperty); }
            set { this.SetValue(TextOpacityProperty, value); }
        }

        public static readonly DependencyProperty TextOpacityProperty = DependencyProperty.Register(
            "TextOpacity", 
            typeof(double), 
            typeof(ClockButton), new PropertyMetadata(1.0));

        public ClockButton(ClockItemMember mode, int value, double centerX, double centerY, bool isInner, IClock owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            this.Mode = mode;
            this.IsInner = isInner;
            this.Value = value;

            this.centerX = centerX;
            this.centerY = centerY;
            this.owner = owner;
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            this.owner.OnButtonTapped(this);
        }

        protected override void OnApplyTemplate()
        {
            Thumb thumb = this.GetTemplateChild("PART_Thumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += this.OnDragStarted;
                thumb.DragDelta += this.OnDragDelta;
                thumb.DragCompleted += this.OnDragCompleted;
            }
        }
        
        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            this.owner.OnButtonDragStarted(this, new DragStartedEventArgs(
                this.centerX + e.HorizontalOffset - this.ActualWidth / 2.0, 
                this.centerY + e.VerticalOffset - this.ActualHeight / 2.0));
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.isDragging = true;
            this.owner.OnButtonDragDelta(this, e);
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (this.isDragging)
                this.owner.OnButtonDragCompleted(this, e);
        }
    }
}