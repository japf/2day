using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class RecurrenceTest : TestCaseBase
    {
        private static readonly DateTime DateSunday17Aug = new DateTime(2014, 08, 17);

        [TestMethod]
        public async Task Every_day()
        {
            await this.Basic_recurrence(FrequencyType.Daily);
        }

        [TestMethod]
        public async Task Every_week()
        {
            await this.Basic_recurrence(FrequencyType.Weekly);
        }

        [TestMethod]
        public async Task Every_month()
        {
            await this.Basic_recurrence(FrequencyType.Monthly);
        }

        [TestMethod]
        public async Task Every_year()
        {
            await this.Basic_recurrence(FrequencyType.Yearly);
        }
  
        [TestMethod]
        public async Task Every_1_day_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Day, 1, true);
        }

        [TestMethod]
        public async Task Every_1_day_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Day, 1, false);
        }

        [TestMethod]
        public async Task Every_3_days_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Day, 3, true);
        }

        [TestMethod]
        public async Task Every_3_days_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Day, 3, false);
        }
        
        [TestMethod]
        public async Task Every_1_week_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Week, 1, true);
        }

        [TestMethod]
        public async Task Every_1_week_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Week, 1, false);
        }

        [TestMethod]
        public async Task Every_3_weeks_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Week, 3, true);
        }

        [TestMethod]
        public async Task Every_3_weeks_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Week, 3, false);
        }
        
        [TestMethod]
        public async Task Every_1_month_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Month, 1, true);
        }

        [TestMethod]
        public async Task Every_1_month_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Month, 1, false);
        }

        [TestMethod]
        public async Task Every_3_months_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Month, 3, true);
        }

        [TestMethod]
        public async Task Every_3_months_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Month, 3, false);
        }

        [TestMethod]
        public async Task Every_1_year_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Year, 1, true);
        }

        [TestMethod]
        public async Task Every_1_year_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Year, 1, false);
        }

        [TestMethod]
        public async Task Every_3_years_fixed()
        {
            if (this.Service == SynchronizationService.ExchangeEws || this.Service == SynchronizationService.Exchange)
                Assert.Inconclusive("Scenario not supported in Exchange");
            else
                await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Year, 3, true);
        }

        [TestMethod]
        public async Task Every_3_years_not_fixed()
        {
            await this.Basic_recurrence(FrequencyType.EveryXPeriod, CustomFrequencyScale.Year, 3, false);
        }

        [TestMethod]
        public async Task Several_days_a_week()
        {
            await this.Basic_recurrence(FrequencyType.DaysOfWeek);
        }

        [TestMethod]
        public async Task Monthly_relative()
        {
            await this.Basic_recurrence(FrequencyType.OnXDayOfEachMonth);
        }

        [TestMethod]
        public void Yearly_relative()
        {
            Assert.Inconclusive("Yearly relative recurrence is not yet supported");
        }

        private async Task Basic_recurrence(FrequencyType frequencyType, CustomFrequencyScale scale = CustomFrequencyScale.Day, int rate = 0, bool useFixedDate = true)
        {
            var folder = this.CreateFolder("folder");

            var task = this.CreateTask("task", folder);
            task.Due = DateSunday17Aug;
            task.FrequencyType = frequencyType;
            task.UseFixedDate = useFixedDate;

            switch (frequencyType)
            {
                case FrequencyType.Daily:
                    task.CustomFrequency = new DailyFrequency();
                    break;
                case FrequencyType.Weekly:
                    task.CustomFrequency = new WeeklyFrequency();
                    break;
                case FrequencyType.Monthly:
                    task.CustomFrequency = new MonthlyFrequency();
                    break;
                case FrequencyType.Yearly:
                    task.CustomFrequency = new YearlyFrequency();
                    break;
                case FrequencyType.DaysOfWeek:
                    task.CustomFrequency = new DaysOfWeekFrequency { IsSunday = true, IsWednesday = true };
                    break;
                case FrequencyType.OnXDayOfEachMonth:
                    task.CustomFrequency = new OnXDayFrequency { DayOfWeek = DayOfWeek.Sunday, RankingPosition = RankingPosition.Third };
                    break;
                case FrequencyType.EveryXPeriod:
                    task.CustomFrequency = new EveryXPeriodFrequency { Scale = scale , Rate= rate };
                    break;
 
                default:
                    throw new ArgumentOutOfRangeException("frequencyType");
            }

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsDue(this.Workbook, 0, task.Due);
            AssertEx.IsRecurringLike(task, this.Workbook.Tasks[0]);
            
            Assert.AreEqual(task.UseFixedDate, this.Workbook.Tasks[0].UseFixedDate);
        }
    }
}
