using System;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsRecurrenceTest : EwsTestBase
    {
        [TestMethod]
        public async Task Daily_recurrence_interval()
        {
            await this.Recurrence_interval(ExchangeRecurrencePattern.Daily);
        }
        
        [TestMethod]
        public async Task Weekly_recurrence_interval()
        {
            await this.Recurrence_interval(ExchangeRecurrencePattern.Weekly, ExchangeDayOfWeek.Saturday);
        }

        [TestMethod]
        public async Task Weekly_recurrence_several_days()
        {
            await this.Recurrence_interval(ExchangeRecurrencePattern.Weekly, ExchangeDayOfWeek.Monday | ExchangeDayOfWeek.Wednesday);
        }

        [TestMethod]
        public async Task Monthly_recurrence_interval()
        {
            await this.Recurrence_interval(ExchangeRecurrencePattern.Monthly, ExchangeDayOfWeek.None, dayOfMonth: 10);
        }

        [TestMethod]
        public async Task Relative_monthly_recurrence_interval()
        {
            await this.Recurrence_interval(ExchangeRecurrencePattern.MonthlyRelative, ExchangeDayOfWeek.Monday, ExchangeDayOfWeekIndex.First);
        }

        [TestMethod]
        public async Task Yearly_recurrence_interval()
        {
            // every 5th
            await this.Recurrence_interval(ExchangeRecurrencePattern.Yearly, null, null, 10, 5);
        }

        [TestMethod]
        public async Task Relative_yearly_recurrence_interval()
        {
            // every second friday of april
            await this.Recurrence_interval(ExchangeRecurrencePattern.YearlyRelative, ExchangeDayOfWeek.Friday, ExchangeDayOfWeekIndex.Second, null, 4);
        }

        [TestMethod]
        public async Task Daily_recurrence_update()
        {
            var ewsTask = CreateSampleEwsTask(titleOnly: true);

            await this.server.CreateItemAsync(ewsTask);

            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            var ewsRecurrence = new EwsRecurrence
            {
                StartDate = DateTime.Now.AddDays(-DateTime.Now.Day + 1),
                RecurrenceType = ExchangeRecurrencePattern.Daily,
                Interval = 2,
            };
            task.Recurrence = ewsRecurrence;
            task.Changes = EwsFields.Recurrence;

            await this.server.UpdateItemAsync(task);

            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsTrue(task.IsRecurring);
            Assert.IsNotNull(task.Recurrence);
            Assert.AreEqual(ewsRecurrence, task.Recurrence);
        }

        [TestMethod]
        public async Task Daily_recurrence_clear()
        {
            var ewsTask = CreateSampleEwsTask(titleOnly: true);
            var ewsRecurrence = new EwsRecurrence
            {
                StartDate = DateTime.Now.AddDays(-DateTime.Now.Day + 1),
                RecurrenceType = ExchangeRecurrencePattern.Daily,
                Interval = 2,
            };
            ewsTask.Recurrence = ewsRecurrence;

            await this.server.CreateItemAsync(ewsTask);

            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.Recurrence = null;
            task.IsRecurring = false;
            task.Changes = EwsFields.Recurrence;

            await this.server.UpdateItemAsync(task);

            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsFalse(task.IsRecurring);
            Assert.IsNull(task.Recurrence);
        }

        private async Task Recurrence_interval(ExchangeRecurrencePattern type, ExchangeDayOfWeek? days = ExchangeDayOfWeek.None, ExchangeDayOfWeekIndex? dayOfWeekIndex = null, int? dayOfMonth = null, int? month = null)
        {
            int[] intervals = new[] {1, 2, 4};
            if (type == ExchangeRecurrencePattern.Yearly || type == ExchangeRecurrencePattern.YearlyRelative)
                intervals = new[] {1};

            foreach (int interval in intervals)
            {
                var ewsTask = CreateSampleEwsTask(titleOnly: true);
                var ewsRecurrence = new EwsRecurrence
                {
                    StartDate = DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1).AddHours(8), // use 8AM to prevent weird issue depending on when the test run
                    RecurrenceType = type,
                    Interval = interval,
                };

                // set a valid start date
                if (dayOfWeekIndex != null)
                {
                    if (month != null)
                        ewsRecurrence.StartDate = ewsRecurrence.StartDate.AddMonths(month.Value - ewsRecurrence.StartDate.Month);

                    string day = days.ToString();
                    int occurence = (int) dayOfWeekIndex.Value + 1;
                    int count = 0;
                    if (ewsRecurrence.StartDate.DayOfWeek.ToString() == day)
                        count = 1;

                    while (ewsRecurrence.StartDate.DayOfWeek.ToString() != day || count != occurence)
                    {
                        ewsRecurrence.StartDate = ewsRecurrence.StartDate.AddDays(1);
                        if (ewsRecurrence.StartDate.DayOfWeek.ToString() == day)
                            count++;
                    }
                }
                else if (days != null && days != ExchangeDayOfWeek.None)
                {
                    string day = days.ToString();
                    if (day.Contains(","))
                        day = day.Split(new [] {','})[0];

                    while (ewsRecurrence.StartDate.DayOfWeek.ToString() != day)
                        ewsRecurrence.StartDate = ewsRecurrence.StartDate.AddDays(1);
                }
                else if (dayOfMonth != null)
                {
                    if (month != null)
                        ewsRecurrence.StartDate = ewsRecurrence.StartDate.AddMonths(month.Value - ewsRecurrence.StartDate.Month);

                    ewsRecurrence.StartDate = ewsRecurrence.StartDate.AddDays(dayOfMonth.Value - ewsRecurrence.StartDate.Day);
                }

                if (days != null)
                    ewsRecurrence.DaysOfWeek = days.Value;
                        
                if (dayOfWeekIndex != null)
                    ewsRecurrence.DayOfWeekIndex = dayOfWeekIndex.Value;
                
                if (dayOfMonth != null)
                    ewsRecurrence.DayOfMonth = dayOfMonth.Value;

                if (month != null)
                    ewsRecurrence.Month = month.Value;

                ewsTask.Recurrence = ewsRecurrence;
                ewsTask.IsRecurring = true;

                await this.server.CreateItemAsync(ewsTask);

                var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

                Assert.IsTrue(task.IsRecurring);
                Assert.IsNotNull(task.Recurrence);
                Assert.AreEqual(ewsRecurrence, task.Recurrence);
            }
        }
    }
}
