using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Manager.UI;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class GridViewActionBar : UserControl
    {
        private GridViewSelectionManager selectionManager;
        private DateTimePicker dateTimePicker;

        public SyncPrioritySupport PriorityMode
        {
            get { return (SyncPrioritySupport)this.GetValue(PriorityModeProperty); }
            set { this.SetValue(PriorityModeProperty, value); }
        }

        public static readonly DependencyProperty PriorityModeProperty = DependencyProperty.Register(
            "PriorityMode",
            typeof(SyncPrioritySupport),
            typeof(GridViewActionBar),
            new PropertyMetadata(SyncPrioritySupport.Normal, OnSelecteModeChanged));

        public GridViewActionBar()
        {
            this.InitializeComponent();            
        }
        
        public void Initialize(GridViewSelectionManager taskListSelectionManager)
        {
            if (taskListSelectionManager == null)
                throw new ArgumentNullException(nameof(taskListSelectionManager));

            this.selectionManager = taskListSelectionManager;
        }

        private static void OnSelecteModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridViewActionBar)d).OnSelecteModeChanged();
        }

        private void OnSelecteModeChanged()
        {
            this.buttonNone.Visibility = Visibility.Visible;
            this.buttonLow.Visibility = Visibility.Visible;
            this.buttonMedium.Visibility = Visibility.Visible;
            this.buttonHigh.Visibility = Visibility.Visible;
            this.buttonStar.Visibility = Visibility.Visible;
            this.buttonHigh.BorderThickness = new Thickness(1, 1, 0, 1);

            if (this.PriorityMode  == SyncPrioritySupport.LowMediumHigh)
            {
                this.buttonNone.Visibility = Visibility.Collapsed;
                this.buttonStar.Visibility = Visibility.Collapsed;
                this.buttonHigh.BorderThickness = new Thickness(1, 1, 1, 1);
            }
        }

        private void OnButtonPostponeTapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.dateTimePicker == null)
            {
                this.dateTimePicker = (DateTimePicker)this.FindName("DateTimePicker");
                this.dateTimePicker.DateChanged += (s, e1) =>
                {
                    if (this.selectionManager != null)
                    {
                        IList<ITask> selectedTasks = this.selectionManager.SelectedTasks;
                        if (selectedTasks.Count > 0)
                        {
                            DateTime? newDate = e1.Item;
                            foreach (var selectedTask in selectedTasks)
                            {
                                selectedTask.SetDueAndAdjustReminder(newDate, selectedTasks, true);
                            }
                        }
                    }
                };
            }
        }
    }
}
