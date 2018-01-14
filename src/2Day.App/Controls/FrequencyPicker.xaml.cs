using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.Behavior;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class FrequencyPicker : UserControl, IFlyoutContent
    {
        private FlyoutBase flyout;
        private TaskViewModelBase viewModel;
        private Frequency everyFrequency;
        private Frequency daysFrequency;
        private Frequency eachFrequency;

        public FrequencyPicker()
        {
            this.InitializeComponent();

            this.txtEvery.Text = StringResources.CustomFrequency_EveryXPeriodTitle;
            this.txtDays.Text = StringResources.CustomFrequency_DaysOfWeekTitle;
            this.txtDaysOfMonth.Text = StringResources.CustomFrequency_XthDayOfTheMonthTitle;

            this.cbDay1.Content = DateTimeExtensions.MondayString;
            this.cbDay2.Content = DateTimeExtensions.TuesdayString;
            this.cbDay3.Content = DateTimeExtensions.WednesdayString;
            this.cbDay4.Content = DateTimeExtensions.ThursdayString;
            this.cbDay5.Content = DateTimeExtensions.FridayString;
            this.cbDay6.Content = DateTimeExtensions.SaturdayString;
            this.cbDay7.Content = DateTimeExtensions.SundayString;
            
            this.cbDays.ItemsSource = FrequencyMetadata.AvailableDays;
            this.cbRankings.ItemsSource = FrequencyMetadata.AvailableRankings;
            this.cbRates.ItemsSource = FrequencyMetadata.AvailableRates;
            this.cbScales.ItemsSource = FrequencyMetadata.AvailableScales;

            if (!DesignMode.DesignModeEnabled)
                this.Loaded += this.OnLoaded;
        }

        public void HandleFlyout(FlyoutBase flyoutBase)
        {
            this.flyout = flyoutBase;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
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
                    this.rbEvery.IsChecked = true;
                }
                else if (frequency.CustomFrequency is DaysOfWeekFrequency)
                {
                    this.daysFrequency = frequency;
                    this.rbDays.IsChecked = true;

                    var daysOfweekFrequency = (DaysOfWeekFrequency)frequency.CustomFrequency;

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
                    this.rbDaysOfMonth.IsChecked = true;
                }

                if (frequency.UseFixedDate)
                    this.rbRepeatDueDate.IsChecked = true;
                else
                    this.rbRepeatTaskCompletes.IsChecked = true;
            }
            else
            {
                this.rbEvery.IsChecked = true;

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

            this.areaEvery.DataContext = this.everyFrequency.CustomFrequency;
            this.areaSpecificDays.DataContext = this.daysFrequency.CustomFrequency;
            this.areaDaysOfMonth.DataContext = this.eachFrequency.CustomFrequency;

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
            if (this.rbEvery.IsChecked.HasValue && this.rbEvery.IsChecked.Value)
            {
                this.everyFrequency.Custom<EveryXPeriodFrequency>().Rate = ((RateItem)this.cbRates.SelectedItem).Value;
                this.everyFrequency.Custom<EveryXPeriodFrequency>().Scale = ((ScaleItem)this.cbScales.SelectedItem).Value;

                this.viewModel.SelectedFrequency = this.everyFrequency;
            }
            else if (this.rbDays.IsChecked.HasValue && this.rbDays.IsChecked.Value && this.daysFrequency.CustomFrequency is DaysOfWeekFrequency)
            {
                var daysOfweekFrequency = (DaysOfWeekFrequency)this.daysFrequency.CustomFrequency;

                daysOfweekFrequency.IsMonday        = this.cbDay1.IsChecked.HasValue && this.cbDay1.IsChecked.Value;
                daysOfweekFrequency.IsTuesday       = this.cbDay2.IsChecked.HasValue && this.cbDay2.IsChecked.Value;
                daysOfweekFrequency.IsWednesday     = this.cbDay3.IsChecked.HasValue && this.cbDay3.IsChecked.Value;
                daysOfweekFrequency.IsThursday      = this.cbDay4.IsChecked.HasValue && this.cbDay4.IsChecked.Value;
                daysOfweekFrequency.IsFriday        = this.cbDay5.IsChecked.HasValue && this.cbDay5.IsChecked.Value;
                daysOfweekFrequency.IsSaturday      = this.cbDay6.IsChecked.HasValue && this.cbDay6.IsChecked.Value;
                daysOfweekFrequency.IsSunday        = this.cbDay7.IsChecked.HasValue && this.cbDay7.IsChecked.Value;

                this.viewModel.SelectedFrequency = this.daysFrequency;
            }
            else if (this.rbDaysOfMonth.IsChecked.HasValue && this.rbDaysOfMonth.IsChecked.Value)
            {
                this.eachFrequency.Custom<OnXDayFrequency>().RankingPosition = ((RankingItem)this.cbRankings.SelectedItem).Value;
                this.eachFrequency.Custom<OnXDayFrequency>().DayOfWeek = ((DayOfWeekItem)this.cbDays.SelectedItem).Value;

                this.viewModel.SelectedFrequency = this.eachFrequency;
            }

            this.viewModel.SelectedFrequency.UseFixedDate = (bool) this.rbRepeatDueDate.IsChecked;

            this.Hide();
        }

        private void OnBtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.flyout.Hide();
        }

        private void Hide()
        {
            if (this.flyout != null)
            {
                this.flyout.Hide();
            }
            else
            {
                if (this.Parent is Popup)
                    ((Popup) this.Parent).IsOpen = false;
            }
        }
    }
}
