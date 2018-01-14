using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.Recurrence
{
    [TestClass]
    public class RecurringTaskHelperTest
    {
        [TestMethod]
        public void Action_field_must_be_copied()
        {
            // setup
            var task = new Task
            {
                Due = DateTime.Now,
                Action = TaskAction.Sms,
                ActionName = "John",
                ActionValue = "012345789",
                CustomFrequency = new DailyFrequency()
            };

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(task.Action, newTask.Action);
            Assert.AreEqual(task.ActionName, newTask.ActionName);
            Assert.AreEqual(task.ActionValue, newTask.ActionValue);
        }

        [TestMethod]
        public void Reminder_must_be_removed_from_existing_task()
        {
            // setup
            var task = new Task
            {
                Due = DateTime.Now.Date,
                Alarm = DateTime.Now.Date.AddDays(-1),
                CustomFrequency = new MonthlyFrequency()
            };

            // act
            RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.IsNull(task.Alarm);
        }

        [TestMethod]
        public void Reminder_must_be_updated_when_task_has_due_date()
        {
            var due = new DateTime(2015, 1, 15);
            var alarm = due.AddDays(-1);

            // setup
            var task = new Task
            {
                Due = due,
                Alarm = alarm,
                CustomFrequency = new MonthlyFrequency(),
                UseFixedDate = true
            };

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(due.AddMonths(1).Date, newTask.Due.Value.Date);
            Assert.AreEqual(alarm.AddMonths(1), newTask.Alarm);
        }

        [TestMethod]
        public void Reminder_must_be_updated_when_task_has_start_date()
        {
            var start = new DateTime(2015, 1, 15);
            var alarm = start.AddDays(-1);

            // setup
            var task = new Task
            {
                Start = start,
                Alarm = alarm,
                CustomFrequency = new MonthlyFrequency(),
                UseFixedDate = true
            };

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(start.AddMonths(1).Date, newTask.Start.Value.Date);
            Assert.AreEqual(alarm.AddMonths(1), newTask.Alarm);
        }

        [TestMethod]
        public void StartDate_must_be_updated_fixed_date()
        {
            var due = new DateTime(2015, 1, 1, 8, 15, 0);
            var start = due.AddDays(-3);

            // setup
            var task = new Task
            {
                Due = due,
                Start = start,
                CustomFrequency = new MonthlyFrequency(),
                UseFixedDate = true
            };

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(start.AddMonths(1), newTask.Start);
        }

        [TestMethod]
        public void StartDate_must_be_updated_no_fixed_date()
        {
            var due = new DateTime(2015, 1, 1, 8, 15, 0);
            var start = due.AddDays(-3);

            // setup
            var task = new Task
            {
                Due = due,
                Start = start,
                CustomFrequency = new MonthlyFrequency(),
                UseFixedDate = false
            };

            var nextExpectedStartDate = DateTime.Now.Date.AddDays(1);
            nextExpectedStartDate = nextExpectedStartDate.AddHours(start.Hour);
            nextExpectedStartDate = nextExpectedStartDate.AddMinutes(start.Minute);
            while (nextExpectedStartDate.Day != DateTime.Now.Day)
                nextExpectedStartDate = nextExpectedStartDate.AddDays(1);

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(nextExpectedStartDate, newTask.Start);
        }

        [TestMethod]
        public void StartDate_no_due()
        {
            // setup
            var start = new DateTime(2015, 1, 1);
            var task = new Task
            {
                Start = start,
                CustomFrequency = new MonthlyFrequency(),
                UseFixedDate = true
            };

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(start.AddMonths(1), newTask.Start);
        }

        [TestMethod]
        public void StartDate_time_with_x_day()
        {
            // setup
            var start = new DateTime(2015, 5, 4, 15, 0, 0); // May 4rd 2015 is a Monday
            var task = new Task
            {
                Start = start,
                CustomFrequency = new OnXDayFrequency { DayOfWeek = DayOfWeek.Monday, RankingPosition = RankingPosition.First },
                UseFixedDate = true
            };

            // act
            var newTask = RecurringTaskHelper.CreateNewTask(task);

            // check
            Assert.AreEqual(new DateTime(2015, 6, 1, 15, 0, 0), newTask.Start);
        }
    }
}
