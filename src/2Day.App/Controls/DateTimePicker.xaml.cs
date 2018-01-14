using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Resources;
using System.Globalization;
using Windows.Globalization.DateTimeFormatting;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class DateTimePicker : UserControl
    {
        private bool isLoadingValues;
        private bool hasInitialDate;

        public bool NoDateSelectionOnLoad { get; set; }

        public bool PickDefaultTime
        {
            get { return (bool)this.GetValue(PickDefaultTimeProperty); }
            set { this.SetValue(PickDefaultTimeProperty, value); }
        }

        public static readonly DependencyProperty PickDefaultTimeProperty = DependencyProperty.Register(
            "PickDefaultTime",
            typeof(bool),
            typeof(DateTimePicker),
            new PropertyMetadata(false));

        public bool ShowTime
        {
            get { return (bool)this.GetValue(ShowTimeProperty); }
            set { this.SetValue(ShowTimeProperty, value); }
        }

        public static readonly DependencyProperty ShowTimeProperty = DependencyProperty.Register(
            "ShowTime", 
            typeof(bool), 
            typeof(DateTimePicker), 
            new PropertyMetadata(true));

        public bool HasTime
        {
            get { return (bool)this.GetValue(HasTimeProperty); }
            set { this.SetValue(HasTimeProperty, value); }
        }

        public static readonly DependencyProperty HasTimeProperty = DependencyProperty.Register(
            "HasTime",
            typeof(bool),
            typeof(DateTimePicker),
            new PropertyMetadata(true));

        public DateTime? Date
        {
            get { return (DateTime?)this.GetValue(DateProperty); }
            set { this.SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
            "Date", 
            typeof(DateTime?), 
            typeof(DateTimePicker), 
            new PropertyMetadata(null));

        public TimeSpan Time
        {
            get { return (TimeSpan)this.GetValue(TimeProperty); }
            set { this.SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time",
            typeof(TimeSpan),
            typeof(DateTimePicker),
            new PropertyMetadata(null));

        public bool AutoCloseParentPopup
        {
            get { return (bool)this.GetValue(AutoCloseParentPopupProperty); }
            set { this.SetValue(AutoCloseParentPopupProperty, value); }
        }

        public static readonly DependencyProperty AutoCloseParentPopupProperty = DependencyProperty.Register(
            "AutoCloseParentPopup",
            typeof(bool),
            typeof(DateTimePicker),
            new PropertyMetadata(true));
        
        public event EventHandler<EventArgs<DateTime?>> DateChanged;

        public DateTimePicker()
        {
            this.InitializeComponent();
            
            if (!DesignMode.DesignModeEnabled)
            {
                var workbook = Ioc.Resolve<IWorkbook>();
                this.CalendarView.FirstDayOfWeek = (Windows.Globalization.DayOfWeek)workbook.Settings.GetValue<DayOfWeek>(CoreSettings.FirstDayOfWeek);
            }

            this.Loaded += this.OnLoaded;

            this.CalendarView.SelectedDatesChanged += this.OnSelectedDateChanged;
            this.TimePicker.TimeChanged += this.OnTimechanged;

            try
            {
                if (GetCurrentCulture().DateTimeFormat.ShortTimePattern.Contains("H"))
                    this.TimePicker.ClockIdentifier = Windows.Globalization.ClockIdentifiers.TwentyFourHour;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Getting short time pattern");
            }
        }

        private static CultureInfo GetCurrentCulture()
        {
            // a hack to get "real" culture when app runs on a machine where it's not localized (example: Finland)
            var cultureName = new DateTimeFormatter("longdate", new[] { "US" }).ResolvedLanguage;
            return new CultureInfo(cultureName);
        }

        private void OnTimechanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (this.ShowTime)
                this.ClosePopup(false);
        }

        private void OnSelectedDateChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            this.ClosePopup(false);
        }

        private void ClosePopup(bool setToNull)
        {
            if (!this.isLoadingValues && this.Parent != null)
            {
                this.Time = this.TimePicker.Time;

                if (setToNull)
                {
                    this.Date = null;
                    this.CalendarView.SelectedDates.Clear();
                    if (this.DateChanged != null)
                        this.DateChanged(this, new EventArgs<DateTime?>(this.Date));
                }
                else if (this.CalendarView.SelectedDates.Count == 1)
                {
                    var dateTime = this.CalendarView.SelectedDates[0].Date;
                    this.Date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Local);
                    if (this.DateChanged != null)
                        this.DateChanged(this, new EventArgs<DateTime?>(this.Date));
                }

                if (this.AutoCloseParentPopup)
                {
                    var parent = this.Parent as FrameworkElement;
                    while (parent != null && !(parent is Popup))
                        parent = parent.Parent as FrameworkElement;

                    if (parent != null)
                        ((Popup) parent).IsOpen = false;
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.isLoadingValues = true;

            if (this.NoDateSelectionOnLoad)
            {
                this.CalendarView.SelectedDates.Clear();
                this.hasInitialDate = this.Date.HasValue;
            }
            else
            {
                if (!this.HasTime)
                {
                    if (this.PickDefaultTime)
                        this.TimePicker.Time = DateTime.Now.Date.TimeOfDay.Add(TimeSpan.FromHours(12)); // default to 12:00
                    else
                        this.TimePicker.Time = DateTime.Now.Date.TimeOfDay;                             // default to 00:00
                }
                else
                {
                    this.TimePicker.Time = this.Time;
                }

                this.CalendarView.SelectedDates.Clear();
                if (this.Date.HasValue)
                {
                    this.CalendarView.SelectedDates.Add(this.Date.Value);
                    this.hasInitialDate = true;
                }
                else
                {
                    this.hasInitialDate = false;
                }
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

        private DateTime GetDate()
        {
            if (this.NoDateSelectionOnLoad && this.Date.HasValue)
                return this.Date.Value;
            if (this.CalendarView.SelectedDates.Count != 1 || !this.hasInitialDate)
                return DateTime.Now;
            else if (this.hasInitialDate)
                return this.CalendarView.SelectedDates[0].DateTime;
            else
                return DateTime.Now;
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
            this.ClosePopup(false);
        }
    }
}
