using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class DateTimePicker2 : UserControl
    {
        private bool isLoadingValues;

        public bool NoDateSelectionOnLoad { get; set; }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public bool PickDefaultTime
        {
            get { return (bool)this.GetValue(PickDefaultTimeProperty); }
            set { this.SetValue(PickDefaultTimeProperty, value); }
        }

        public static readonly DependencyProperty PickDefaultTimeProperty = DependencyProperty.Register(
            "PickDefaultTime",
            typeof(bool),
            typeof(DateTimePicker2),
            new PropertyMetadata(false));

        public bool ShowTime
        {
            get { return (bool)this.GetValue(ShowTimeProperty); }
            set { this.SetValue(ShowTimeProperty, value); }
        }

        public static readonly DependencyProperty ShowTimeProperty = DependencyProperty.Register(
            "ShowTime", 
            typeof(bool), 
            typeof(DateTimePicker2), 
            new PropertyMetadata(true));

        public bool HasTime
        {
            get { return (bool)this.GetValue(HasTimeProperty); }
            set { this.SetValue(HasTimeProperty, value); }
        }

        public static readonly DependencyProperty HasTimeProperty = DependencyProperty.Register(
            "HasTime",
            typeof(bool),
            typeof(DateTimePicker2),
            new PropertyMetadata(true));

        public DateTime? Date
        {
            get { return (DateTime?)this.GetValue(DateProperty); }
            set { this.SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
            "Date", 
            typeof(DateTime?), 
            typeof(DateTimePicker2), 
            new PropertyMetadata(null));

        public TimeSpan Time
        {
            get { return (TimeSpan)this.GetValue(TimeProperty); }
            set { this.SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time",
            typeof(TimeSpan),
            typeof(DateTimePicker2),
            new PropertyMetadata(null));
        
        public DateTimePicker2()
        {
            this.InitializeComponent();
            
            if (!DesignMode.DesignModeEnabled)
            {
                var workbook = Ioc.Resolve<IWorkbook>();
                this.CalendarView.FirstDayOfWeek = (Windows.Globalization.DayOfWeek)workbook.Settings.GetValue<DayOfWeek>(CoreSettings.FirstDayOfWeek);
            }

            this.Loaded += this.OnLoaded;

            this.CalendarView.SelectedDatesChanged += this.OnSelectedDateChanged;
            this.Clock.TimeSelected += this.OnTimeSelected;
            this.ClockDoneButton.Tapped += this.OnClockDoneButtonTapped;
        }

        private void OnClockDoneButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            this.ClosePopup(false);
        }

        private void OnTimeSelected(object sender, EventArgs e)
        {
            if (this.ShowTime)
                this.ClosePopup(false);
        }

        private void OnSelectedDateChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            this.HandleDateSelected();            
        }

        private void ClosePopup(bool setToNull)
        {
            if (!this.isLoadingValues && this.Parent != null)
            {
                this.Time = this.Clock.Time.TimeOfDay;

                if (setToNull)
                {
                    this.Date = null;
                    this.CalendarView.SelectedDates.Clear();
                }
                else if (this.CalendarView.SelectedDates.Count == 1)
                {
                    var dateTime = this.CalendarView.SelectedDates[0].Date;
                    this.Date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Local);
                }

                var popup = this.FindParent<Popup>();
                if (popup != null)
                    popup.IsOpen = false;                
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.isLoadingValues = true;

            VisualStateManager.GoToState(this, "Normal", true);
            this.Clock.DisplayMode = ClockItemMember.Hours;

            if (!this.ShowTime)
            {
                this.TextBlockDate.Visibility = Visibility.Collapsed;
                this.TextBlockTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.TextBlockTime.Visibility = Visibility.Visible;                
                this.TextBlockTime.Visibility = Visibility.Visible;                
            }

            if (this.NoDateSelectionOnLoad)
            {
                this.CalendarView.SelectedDates.Clear();
            }
            else
            {
                if (!this.HasTime)
                {
                    if (this.PickDefaultTime)
                        this.Clock.Time = DateTime.Now.Date.Add(TimeSpan.FromHours(12)); // default to 12:00
                    else
                        this.Clock.Time = DateTime.Now.Date;                             // default to 00:00
                }
                else
                {
                    this.Clock.Time = DateTime.Now.Date.Add(this.Time);
                }

                this.CalendarView.SelectedDates.Clear();
                if (this.Date.HasValue)
                    this.CalendarView.SelectedDates.Add(this.Date.Value);
            }

            this.isLoadingValues = false;

            this.SetupShiftButtonTexts();
        }

        private void SetupShiftButtonTexts()
        {
            var now = DateTime.Now;

            this.textblockDayToday.Text = now.ToString("ddd");
            this.textblockDay1Day.Text = now.AddDays(1).ToString("ddd");
            this.textblockDay2Days.Text = now.AddDays(2).ToString("ddd");
            this.textblockDay3Days.Text = now.AddDays(3).ToString("ddd");
            this.textblockDay1Week.Text = StringResources.TaskDateShifter_ShiftOneWeek;
            this.textblockDay2Weeks.Text = StringResources.TaskDateShifter_ShiftTwoWeeks;
            this.textblockDay1Month.Text = StringResources.TaskDateShifter_ShiftOneMonth;
            this.textblockDayNone.Text = StringResources.TaskDateShifter_ShiftNone;

            this.textblockDateToday.Text = StringResources.TaskDateShifter_ShiftToday;
            this.textblockDate1Day.Text = StringResources.TaskDateShifter_ShiftTomorrow;
            this.textblockDate2Days.Text = now.AddDays(2).Day.ToString();
            this.textblockDate3Days.Text = now.AddDays(3).Day.ToString();
            this.textblockDate1Week.Text = "+1";
            this.textblockDate2Weeks.Text = "+2";
            this.textblockDate1Month.Text = "+1";
            this.textblockDateNone.Text = "-";
        }
        
        private void ButtonShiftToday_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now);
        }

        private void ButtonShift1Day_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now.AddDays(1));
        }

        private void ButtonShift2Days_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now.AddDays(2));
        }

        private void ButtonShift3Days_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now.AddDays(3));
        }

        private void ButtonShift1Week_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now.AddDays(7));
        }

        private void ButtonShift2Weeks_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now.AddDays(14));
        }

        private void ButtonShift1Month_Click(object sender, RoutedEventArgs e)
        {
            this.SetSelectedDate(DateTime.Now.AddMonths(1));
        }

        private void ButtonShiftNone_Click(object sender, RoutedEventArgs e)
        {
            this.ClosePopup(true);
        }

        private void SetSelectedDate(DateTimeOffset date)
        {
            this.isLoadingValues = true;
            this.CalendarView.SelectedDates.Clear();
            this.isLoadingValues = false;

            this.CalendarView.SelectedDates.Add(date);
            this.HandleDateSelected();
        }

        private void HandleDateSelected()
        {
            if (this.isLoadingValues)
                return;

            if (this.ShowTime)
                VisualStateManager.GoToState(this, "TimeState", true);
            else
                this.ClosePopup(false);
        }

        private void OnTextBlockDateTapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void OnTextBlockTimeTapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "TimeState", true);
        }
    }
}
