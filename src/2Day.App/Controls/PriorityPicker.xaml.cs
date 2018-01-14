using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.App.Controls
{
    public partial class PriorityPicker : UserControl
    {
        public TaskPriority SelectedPriority
        {
            get { return (TaskPriority) this.GetValue(SelectedPriorityProperty); }
            set { this.SetValue(SelectedPriorityProperty, value); }
        }

        public static readonly DependencyProperty SelectedPriorityProperty = DependencyProperty.Register(
            "SelectedPriority",
            typeof(TaskPriority),
            typeof(PriorityPicker),
            new PropertyMetadata(TaskPriority.None, OnSelectedPriorityChanged));

        public SyncPrioritySupport Mode
        {
            get { return (SyncPrioritySupport) this.GetValue(ModeProperty); }
            set { this.SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode",
            typeof(SyncPrioritySupport),
            typeof(PriorityPicker),
            new PropertyMetadata(SyncPrioritySupport.Normal, OnSelecteModeChanged));
        
        public PriorityPicker()
        {
            this.InitializeComponent();

            this.OnSelectedPriorityChanged();
        }

        private static void OnSelectedPriorityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PriorityPicker)d).OnSelectedPriorityChanged();
        }

        private void OnSelectedPriorityChanged()
        {
            this.buttonNone.IsChecked = false;
            this.buttonLow.IsChecked = false;
            this.buttonMedium.IsChecked = false;
            this.buttonHigh.IsChecked = false;
            this.buttonStar.IsChecked = false;

            switch (this.SelectedPriority)
            {
                case TaskPriority.None:
                    this.buttonNone.IsChecked = true;
                    break;
                case TaskPriority.Low:
                    this.buttonLow.IsChecked = true;
                    break;
                case TaskPriority.Medium:
                    this.buttonMedium.IsChecked = true;
                    break;
                case TaskPriority.High:
                    this.buttonHigh.IsChecked = true;
                    break;
                case TaskPriority.Star:
                    this.buttonStar.IsChecked = true;
                    break;
            }
        }

        private static void OnSelecteModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PriorityPicker)d).OnSelecteModeChanged();
        }

        private void OnSelecteModeChanged()
        {
            this.buttonNone.Visibility = Visibility.Visible;
            this.buttonLow.Visibility = Visibility.Visible;
            this.buttonMedium.Visibility = Visibility.Visible;
            this.buttonHigh.Visibility = Visibility.Visible;
            this.buttonStar.Visibility = Visibility.Visible;

            foreach (var columnDefinition in this.Grid.ColumnDefinitions)
                columnDefinition.Width = new GridLength(1, GridUnitType.Star);    

            if (this.Mode == SyncPrioritySupport.LowMediumHigh)
            {
                this.buttonNone.Visibility = Visibility.Collapsed;
                this.Grid.ColumnDefinitions[0].Width = new GridLength(0);
                this.buttonHigh.BorderThickness = new Thickness(1);
                this.buttonStar.Visibility = Visibility.Collapsed;
                this.Grid.ColumnDefinitions[4].Width = new GridLength(0);
            }
        }

        private void ButtonPriorityNone_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedPriority = TaskPriority.None;
            this.TryClose();
        }

        private void ButtonPriorityLow_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedPriority = TaskPriority.Low;
            this.TryClose();
        }

        private void ButtonPriorityMedium_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedPriority = TaskPriority.Medium;
            this.TryClose();
        }

        private void ButtonPriorityHigh_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedPriority = TaskPriority.High;
            this.TryClose();
        }

        private void ButtonPriorityStar_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedPriority = TaskPriority.Star;
            this.TryClose();
        }

        private void TryClose()
        {
            var popup = this.FindParent<Popup>();
            if (popup != null)
                popup.IsOpen = false;
        }
    }
}
