using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Model.Recurrence;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class FrequencyPicker2 : UserControl
    {
        private const string StateNormal = "Normal";
        private const string StateEach = "Each";
        private const string StateDaysOfWeek = "DaysOfWeek";
        private const string StateDayOfMonth = "DayOfMonth";

        private TaskViewModelBase viewModel;
        private Frequency everyFrequency;
        private Frequency daysFrequency;
        private Frequency eachFrequency;

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public FrequencyPicker2()
        {
            this.InitializeComponent();

            this.cbDays.ItemsSource = FrequencyMetadata.AvailableDays;
            this.cbRankings.ItemsSource = FrequencyMetadata.AvailableRankings;
            this.cbRates.ItemsSource = FrequencyMetadata.AvailableRates;
            this.cbScales.ItemsSource = FrequencyMetadata.AvailableScales;

            if (!DesignMode.DesignModeEnabled)
                this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.GoToState(StateNormal);

            this.viewModel = (TaskViewModelBase) this.DataContext;

            this.everyFrequency = null;
            this.daysFrequency = null;
            this.eachFrequency = null;

            this.cbDay1.IsChecked = false;
            this.cbDay2.IsChecked = false;
            this.cbDay3.IsChecked = false;
            this.cbDay4.IsChecked = false;
            this.cbDay5.IsChecked = false;
            this.cbDay6.IsChecked = false;
            this.cbDay7.IsChecked = false;

            var selectedFrequency = this.viewModel.SelectedFrequency;
            if (selectedFrequency != null && selectedFrequency.IsCustom)
            {
                var frequency = selectedFrequency;

                if (frequency.CustomFrequency is EveryXPeriodFrequency)
                {
                    this.everyFrequency = frequency;
                    this.GoToState(StateEach);
                }
                else if (frequency.CustomFrequency is DaysOfWeekFrequency)
                {
                    this.daysFrequency = frequency;
                    this.GoToState(StateDaysOfWeek);

                    var daysOfweekFrequency = (DaysOfWeekFrequency) frequency.CustomFrequency;

                    this.cbDay1.IsChecked = daysOfweekFrequency.IsMonday;
                    this.cbDay2.IsChecked = daysOfweekFrequency.IsTuesday;
                    this.cbDay3.IsChecked = daysOfweekFrequency.IsWednesday;
                    this.cbDay4.IsChecked = daysOfweekFrequency.IsThursday;
                    this.cbDay5.IsChecked = daysOfweekFrequency.IsFriday;
                    this.cbDay6.IsChecked = daysOfweekFrequency.IsSaturday;
                    this.cbDay7.IsChecked = daysOfweekFrequency.IsSunday;
                }
                else if (frequency.CustomFrequency is OnXDayFrequency)
                {
                    this.eachFrequency = frequency;
                    this.GoToState(StateDayOfMonth);
                }

                if (frequency.UseFixedDate)
                    this.rbRepeatDueDate.IsChecked = true;
                else
                    this.rbRepeatTaskCompletes.IsChecked = true;
            }
            else if (selectedFrequency == null || (selectedFrequency != null && selectedFrequency.CustomFrequency is OnceOnlyFrequency))
            {
                this.GoToState(StateNormal);
            }
            else
            {
                this.GoToState(StateEach);

                if (selectedFrequency != null)
                {
                    if (selectedFrequency.CustomFrequency is DailyFrequency)
                    {
                        this.cbRates.SelectedItem = FrequencyMetadata.AvailableRates.FirstOrDefault(rate => rate.Value == 1);
                        this.cbScales.SelectedItem = FrequencyMetadata.AvailableScales.FirstOrDefault(scale => scale.Value == CustomFrequencyScale.Day);
                    }
                    else if (selectedFrequency.CustomFrequency is WeeklyFrequency)
                    {
                        this.cbRates.SelectedItem = FrequencyMetadata.AvailableRates.FirstOrDefault(rate => rate.Value == 1);
                        this.cbScales.SelectedItem = FrequencyMetadata.AvailableScales.FirstOrDefault(scale => scale.Value == CustomFrequencyScale.Week);
                    }
                    else if (selectedFrequency.CustomFrequency is MonthlyFrequency)
                    {
                        this.cbRates.SelectedItem = FrequencyMetadata.AvailableRates.FirstOrDefault(rate => rate.Value == 1);
                        this.cbScales.SelectedItem = FrequencyMetadata.AvailableScales.FirstOrDefault(scale => scale.Value == CustomFrequencyScale.Month);
                    }
                    else if (selectedFrequency.CustomFrequency is YearlyFrequency)
                    {
                        this.cbRates.SelectedItem = FrequencyMetadata.AvailableRates.FirstOrDefault(rate => rate.Value == 1);
                        this.cbScales.SelectedItem = FrequencyMetadata.AvailableScales.FirstOrDefault(scale => scale.Value == CustomFrequencyScale.Year);
                    }
                }

                if (selectedFrequency != null && !selectedFrequency.UseFixedDate)
                    this.rbRepeatTaskCompletes.IsChecked = true;
                else
                    this.rbRepeatDueDate.IsChecked = true;
            }

            if (this.everyFrequency == null)
                this.everyFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.EveryXPeriod));

            if (this.daysFrequency == null)
                this.daysFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.DaysOfWeek));

            if (this.eachFrequency == null)
                this.eachFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.OnXDayOfEachMonth));

            this.PART_Each.DataContext = this.everyFrequency.CustomFrequency;
            this.PART_DaysOfWeek.DataContext = this.daysFrequency.CustomFrequency;
            this.PART_DayOfMonth.DataContext = this.eachFrequency.CustomFrequency;

            if (this.cbRates.SelectedItem == null)
                this.cbRates.SelectedItem = FrequencyMetadata.AvailableRates.FirstOrDefault(rate => rate.Value == this.everyFrequency.Custom<EveryXPeriodFrequency>().Rate);

            if (this.cbScales.SelectedItem == null)
                this.cbScales.SelectedItem = FrequencyMetadata.AvailableScales.FirstOrDefault(scale => scale.Value == this.everyFrequency.Custom<EveryXPeriodFrequency>().Scale);

            this.cbRankings.SelectedItem = FrequencyMetadata.AvailableRankings.FirstOrDefault(ranking => ranking.Value == this.eachFrequency.Custom<OnXDayFrequency>().RankingPosition);
            this.cbDays.SelectedItem = FrequencyMetadata.AvailableDays.FirstOrDefault(day => day.Value == this.eachFrequency.Custom<OnXDayFrequency>().DayOfWeek);
        }

        private void OnBtnValidateClick(object sender, RoutedEventArgs e)
        {
            // apply changes
            if (this.CommonStates?.CurrentState?.Name == StateEach)
            {
                this.everyFrequency.Custom<EveryXPeriodFrequency>().Rate = ((RateItem) this.cbRates.SelectedItem).Value;
                this.everyFrequency.Custom<EveryXPeriodFrequency>().Scale = ((ScaleItem) this.cbScales.SelectedItem).Value;

                this.viewModel.SelectedFrequency = this.everyFrequency;
            }
            else if (this.CommonStates?.CurrentState?.Name == StateDaysOfWeek && this.daysFrequency.CustomFrequency is DaysOfWeekFrequency)
            {
                var daysOfweekFrequency = (DaysOfWeekFrequency) this.daysFrequency.CustomFrequency;

                daysOfweekFrequency.IsMonday = this.cbDay1.IsChecked.HasValue && this.cbDay1.IsChecked.Value;
                daysOfweekFrequency.IsTuesday = this.cbDay2.IsChecked.HasValue && this.cbDay2.IsChecked.Value;
                daysOfweekFrequency.IsWednesday = this.cbDay3.IsChecked.HasValue && this.cbDay3.IsChecked.Value;
                daysOfweekFrequency.IsThursday = this.cbDay4.IsChecked.HasValue && this.cbDay4.IsChecked.Value;
                daysOfweekFrequency.IsFriday = this.cbDay5.IsChecked.HasValue && this.cbDay5.IsChecked.Value;
                daysOfweekFrequency.IsSaturday = this.cbDay6.IsChecked.HasValue && this.cbDay6.IsChecked.Value;
                daysOfweekFrequency.IsSunday = this.cbDay7.IsChecked.HasValue && this.cbDay7.IsChecked.Value;

                this.viewModel.SelectedFrequency = this.daysFrequency;
            }
            else if (this.CommonStates?.CurrentState?.Name == StateDayOfMonth)
            {
                this.eachFrequency.Custom<OnXDayFrequency>().RankingPosition = ((RankingItem) this.cbRankings.SelectedItem).Value;
                this.eachFrequency.Custom<OnXDayFrequency>().DayOfWeek = ((DayOfWeekItem) this.cbDays.SelectedItem).Value;

                this.viewModel.SelectedFrequency = this.eachFrequency;
            }

            this.viewModel.SelectedFrequency.UseFixedDate = (bool) this.rbRepeatDueDate.IsChecked;

            this.Hide();
        }

        private void OnBtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Hide()
        {
            var popup = this.FindParent<Popup>();
            if (popup != null)
                popup.IsOpen = false;
        }

        private void OnBtnEachTapped(object sender, TappedRoutedEventArgs e)
        {
            this.GoToState(StateEach);
        }

        private void OnBtnDaysOfWeekTapped(object sender, TappedRoutedEventArgs e)
        {
            this.GoToState(StateDaysOfWeek);
        }

        private void OnBtnDayOfMonthTapped(object sender, TappedRoutedEventArgs e)
        {
            this.GoToState(StateDayOfMonth);
        }

        private void OnBtnNoneTapped(object sender, TappedRoutedEventArgs e)
        {
            this.viewModel.SelectedFrequency = null;
            this.Hide();
        }

        private void OnBtnEveryDayTapped(object sender, TappedRoutedEventArgs e)
        {
            this.viewModel.SelectedFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Daily));
            this.Hide();
        }

        private void OnBtnEveryWeekTapped(object sender, TappedRoutedEventArgs e)
        {
            this.viewModel.SelectedFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Weekly));
            this.Hide();
        }

        private void OnBtnEveryMonthTapped(object sender, TappedRoutedEventArgs e)
        {
            this.viewModel.SelectedFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Monthly));
            this.Hide();
        }

        private void OnBtnEveryYearTapped(object sender, TappedRoutedEventArgs e)
        {
            this.viewModel.SelectedFrequency = new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Yearly));
            this.Hide();
        }

        private void OnBtnGoBackTapped(object sender, TappedRoutedEventArgs e)
        {
            this.GoToState(StateNormal);
        }

        private void GoToState(string state)
        {
            VisualStateManager.GoToState(this, state, true);
        }
    }
}