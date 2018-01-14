using Windows.UI.Xaml.Controls.Primitives;

namespace Chartreuse.Today.App.Controls
{
    public interface IClock
    {
        void OnButtonTapped(ClockButton sender);
        void OnButtonDragStarted(ClockButton sender, DragStartedEventArgs e);
        void OnButtonDragDelta(ClockButton sender, DragDeltaEventArgs e);
        void OnButtonDragCompleted(ClockButton sender, DragCompletedEventArgs e);
    }
}