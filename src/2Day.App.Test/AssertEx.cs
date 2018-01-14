using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Chartreuse.Today.App.Test
{
    public static class AssertEx
    {
        public static void ArraysAreEquals<T>(T[] expected, T[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        public static void DateAreEquals(DateTime? expected, DateTime? actual, bool skipTime = false)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                if (skipTime)
                    Assert.AreEqual(expected.Value.Date, actual.Value.Date);
                else
                    Assert.IsTrue(Math.Abs((expected.Value - actual.Value).TotalSeconds) < 1);
            }
        }

        public static void CheckSyncId(IWorkbook workbook)
        {
            Assert.IsTrue(workbook.Tasks.All(t => !string.IsNullOrEmpty(t.SyncId)));
            Assert.AreEqual(workbook.Tasks.Count, workbook.Tasks.Select(t => t.SyncId).Distinct().Count());
        }

        public static void ContainsFolders(IWorkbook workbook, params string[] folders)
        {
            ContainsFolders(workbook.Folders.Select(f => f.Name).ToList(), folders);
        }

        public static void ContainsFolders(Task<List<string>> source, params string[] folders)
        {
            source.ContinueWith(r => ContainsFolders(r.Result, folders));
        }

        public static void ContainsFolders(IList<string> source, params string[] folders)
        {
            // order matters
            Assert.AreEqual(source.Count, folders.Length);
            for (int i = 0; i < folders.Length; i++)
            {
                Assert.AreEqual(source[i], folders[i]);
            }
        }

        public static void ContainsContexts(IWorkbook workbook, params string[] contexts)
        {
            // order matters
            Assert.AreEqual(workbook.Contexts.Count, contexts.Length);
            for (int i = 0; i < contexts.Length; i++)
            {
                Assert.AreEqual(workbook.Contexts[i].Name, contexts[i]);
            }
        }

        public static List<ITask> ContainsTasks(IWorkbook workbook, params string[] tasks)
        {
            // order matters
            Assert.AreEqual(tasks.Length, workbook.Tasks.Count);
            for (int i = 0; i < tasks.Length; i++)
            {
                Assert.AreEqual(tasks[i], workbook.Tasks[i].Title);
            }

            return workbook.Tasks.ToList();
        }

        public static ITask ContainsTask(IWorkbook workbook, string name, int dayOfMonth)
        {
            var task = workbook.Tasks.FirstOrDefault(t => t.Due.HasValue && t.Due.Value.Date.Day == dayOfMonth);

            Assert.IsNotNull(task);
            Assert.AreEqual(name, task.Title);

            return task;
        }

        public static void IsCompleted(IWorkbook workbook, int taskIndex, DateTime? completionDate)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);

            Assert.AreEqual(completionDate.HasValue, workbook.Tasks[taskIndex].Completed.HasValue);
            if (completionDate.HasValue)
                Assert.AreEqual(workbook.Tasks[taskIndex].Completed.Value.Date, completionDate.Value.Date);
        }

        public static void IsDue(IWorkbook workbook, int taskIndex, DateTime? dueDate)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);

            Assert.AreEqual(dueDate.HasValue, workbook.Tasks[taskIndex].Due.HasValue);
            if (dueDate.HasValue)
                Assert.AreEqual(workbook.Tasks[taskIndex].Due.Value.Date, dueDate.Value.Date);
        }

        public static void IsWithNote(IWorkbook workbook, int taskIndex, string note)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);
            Assert.AreEqual(note, workbook.Tasks[taskIndex].Note);
        }

        public static void IsStarting(IWorkbook workbook, int taskIndex, DateTime? startDate)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);

            Assert.AreEqual(startDate.HasValue, workbook.Tasks[taskIndex].Start.HasValue);
            if (startDate.HasValue)
                Assert.AreEqual(workbook.Tasks[taskIndex].Start.Value.Date, startDate.Value.Date);
        }

        public static void IsAlarming(IWorkbook workbook, int taskIndex, DateTime? alarm)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);

            Assert.AreEqual(alarm.HasValue, workbook.Tasks[taskIndex].Alarm.HasValue);
            if (alarm.HasValue)
                Assert.AreEqual(workbook.Tasks[taskIndex].Alarm.Value.Date, alarm.Value.Date);
        }

        public static void IsRepeat(IWorkbook workbook, int taskIndex, FrequencyType? frequencyType)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);

            Assert.AreEqual(frequencyType.HasValue, workbook.Tasks[taskIndex].FrequencyType.HasValue);
            if (frequencyType.HasValue)
                Assert.AreEqual(workbook.Tasks[taskIndex].FrequencyType.Value, frequencyType.Value);
        }

        public static void IsRepeatLike(IWorkbook workbook, int taskIndex, ITask task)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);
            Assert.IsTrue(task.CustomFrequency.Equals(workbook.Tasks[taskIndex].CustomFrequency));
        }

        public static void IsWithPriority(IWorkbook workbook, int taskIndex, TaskPriority priority)
        {
            Assert.IsTrue(taskIndex < workbook.Tasks.Count);
            Assert.AreEqual(priority, workbook.Tasks[0].Priority);
        }

        public static void IsCompleted(ITask task)
        {
            Assert.IsTrue(task.IsCompleted);
        }

        public static void IsDue(ITask task, DateTime? due)
        {
            Assert.AreEqual(task.Due.HasValue, due.HasValue);
            if (due.HasValue)
                Assert.AreEqual(due.Value.Date, task.Due.Value.Date);
        }

        public static void IsRecurringWithFrequencyType(ITask task, FrequencyType? frequencyType)
        {
            Assert.AreEqual(frequencyType, task.FrequencyType);
        }

        public static void IsRecurringWithFrequency(ITask task, ICustomFrequency customFrequency)
        {
            Assert.IsTrue(customFrequency.Equals(task.CustomFrequency));
        }

        public static void IsRecurringLike(ITask expected, ITask actual)
        {
            if (expected.FrequencyType == actual.FrequencyType)
            {
                if (actual.CustomFrequency != null)
                {
                    Assert.IsNotNull(expected.CustomFrequency);
                    Assert.IsTrue(actual.CustomFrequency.Equals(expected.CustomFrequency));
                }
                else
                {
                    Assert.IsNull(expected.CustomFrequency);
                }
            }
            else if (expected.FrequencyType == FrequencyType.EveryXPeriod)
            {
                // second chance for compatible recurrence
                var frequency = (EveryXPeriodFrequency) expected.CustomFrequency;
                if (frequency.Rate == 1)
                {
                    if (frequency.Scale == CustomFrequencyScale.Day)
                        Assert.AreEqual(FrequencyType.Daily, actual.FrequencyType);
                    else if (frequency.Scale == CustomFrequencyScale.Week)
                        Assert.AreEqual(FrequencyType.Weekly, actual.FrequencyType);
                    else if (frequency.Scale == CustomFrequencyScale.Month)
                        Assert.AreEqual(FrequencyType.Monthly, actual.FrequencyType);
                    else if (frequency.Scale == CustomFrequencyScale.Year)
                        Assert.AreEqual(FrequencyType.Yearly, actual.FrequencyType);
                    else
                        Assert.IsFalse(true, "Frequencies does not match");                    
                }
                else
                {
                    Assert.IsFalse(true, "Frequencies does not match");                    
                }
            }
            else
            {
                Assert.IsFalse(true, "Frequencies does not match");
            }
        }
    }
}