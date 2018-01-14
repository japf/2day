using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange
{
    public static class ExchangeHelper
    {
        public static ExchangeTask ToExchangeTask(this ITask task, bool setCategory, TaskProperties properties)
        {
            var exchangeTask = new ExchangeTask
                                   {
                                       Subject = task.Title,
                                       Importance = task.Priority.GetImportance(),
                                       Completed = task.Completed,
                                       Due = task.Due,
                                       Start = task.Start,
                                       LocalId = task.Id,
                                       Id = task.SyncId,
                                       Note = task.Note,
                                       Created = task.Added,
                                       Alarm = task.Alarm,
                                       Properties = (ExchangeTaskProperties?) properties
                                   };

            // flag must be set to false when the folder that owns the task is the default
            // folder we create for task without category in Exchange
            if (setCategory)
                exchangeTask.Category = task.Folder.Name;

            if (task.Progress.HasValue)
                exchangeTask.ProgressPercent = task.Progress.Value;

            if (task.IsPeriodic && task.Due.HasValue)
            {
                exchangeTask.IsRecurring = true;
                exchangeTask.UseFixedDate = task.UseFixedDate;

                if (task.CustomFrequency is DailyFrequency)
                {
                    var frequency = (DailyFrequency) task.CustomFrequency;

                    exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Daily;
                    exchangeTask.Interval = 1;
                }
                else if (task.CustomFrequency is DaysOfWeekFrequency)
                {
                    var frequency = (DaysOfWeekFrequency) task.CustomFrequency;

                    exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Weekly;
                    exchangeTask.Interval = 1;

                    if (frequency.IsMonday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Monday;
                    if (frequency.IsTuesday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Tuesday;
                    if (frequency.IsWednesday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Wednesday;
                    if (frequency.IsThursday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Thursday;
                    if (frequency.IsFriday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Friday;
                    if (frequency.IsSaturday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Saturday;
                    if (frequency.IsSunday)
                        exchangeTask.DaysOfWeek |= ExchangeDayOfWeek.Sunday;
                }
                else if (task.CustomFrequency is EveryXPeriodFrequency)
                {
                    var frequency = (EveryXPeriodFrequency) task.CustomFrequency;
                    exchangeTask.Interval = frequency.Rate;
 
                    switch (frequency.Scale)
                    {
                        case CustomFrequencyScale.Day:
                            if (exchangeTask.UseFixedDate)
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Daily;
                            }
                            else
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.DailyRegeneration;
                            }
                            break;
                        case CustomFrequencyScale.Week:
                            if (exchangeTask.UseFixedDate)
                            {
                                exchangeTask.DaysOfWeek = task.Due.Value.DayOfWeek.ToExchangeDayOfWeek();
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Weekly;
                            }
                            else
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.WeeklyRegeneration;
                            }
                            break;
                        case CustomFrequencyScale.Month:
                            if (exchangeTask.UseFixedDate)
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Monthly;
                                exchangeTask.DayOfMonth = task.Due.Value.Day;
                            }
                            else
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.MonthlyRegeneration;
                            }
                            break;
                        case CustomFrequencyScale.Year:
                            if (exchangeTask.UseFixedDate)
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Yearly;
                                exchangeTask.DayOfMonth = task.Due.Value.Day;
                                exchangeTask.Month = task.Due.Value.Month;
                            }
                            else
                            {
                                exchangeTask.RecurrenceType = ExchangeRecurrencePattern.YearlyRegeneration;
                            }
                            break;
                    }
                }
                else if (task.CustomFrequency is WeeklyFrequency)
                {
                    var frequency = (WeeklyFrequency) task.CustomFrequency;

                    exchangeTask.IsRecurring = true;
                    if (!task.Start.HasValue)
                        exchangeTask.DaysOfWeek = task.Due.Value.DayOfWeek.ToExchangeDayOfWeek();
                    else
                        exchangeTask.DaysOfWeek = task.Start.Value.DayOfWeek.ToExchangeDayOfWeek();
                    exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Weekly;
                    exchangeTask.Interval = 1;
                }
                else if (task.CustomFrequency is MonthlyFrequency && task.Due.HasValue)
                {
                    var frequency = (MonthlyFrequency) task.CustomFrequency;

                    exchangeTask.IsRecurring = true;
                    exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Monthly;
                    exchangeTask.Interval = 1;
                    exchangeTask.DayOfMonth = task.Due.Value.Date.Day;
                }
                else if (task.CustomFrequency is OnXDayFrequency)
                {
                    var frequency = (OnXDayFrequency) task.CustomFrequency;

                    exchangeTask.RecurrenceType = ExchangeRecurrencePattern.MonthlyRelative;
                    exchangeTask.Interval = 1;
                    exchangeTask.DaysOfWeek = frequency.DayOfWeek.ToExchangeDayOfWeek();
                    exchangeTask.DayOfWeekIndex = frequency.RankingPosition.ToExchangeDayIndex();
                }
                else if (task.CustomFrequency is YearlyFrequency && task.Due.HasValue)
                {
                    var frequency = (YearlyFrequency) task.CustomFrequency;

                    exchangeTask.IsRecurring = true;
                    exchangeTask.RecurrenceType = ExchangeRecurrencePattern.Yearly;
                    exchangeTask.Interval = 1;
                    exchangeTask.DayOfMonth = task.Due.Value.Date.Day;
                    exchangeTask.Month = task.Due.Value.Date.Month;
                }
            }

            return exchangeTask;
        }

        public static ITask ToTask(this ExchangeTask exchangeTask, IWorkbook workbook)
        {
            var task = workbook.CreateTask();

            task.Added = DateTime.Now;
            task.UpdateFromExchange(exchangeTask);

            return task;
        }

        public static void UpdateFromExchange(this ITask task, ExchangeTask exchangeTask)
        {
            task.Title = exchangeTask.Subject;
            task.Priority = exchangeTask.Importance.GetPriority();
            task.Completed = exchangeTask.Completed;
            task.Due = exchangeTask.Due;
            task.Start = exchangeTask.Start;

            task.SyncId = exchangeTask.Id;
            task.Note = exchangeTask.Note;
            task.Alarm = exchangeTask.Alarm;

            // set the progress in the task if the value is exchange is more than 0 (ie, the user has set a value - 0 being the default value for all tasks)
            // or if the task has a value which is different from the value in exchange
            if (exchangeTask.ProgressPercent > 0 || (task.Progress.HasValue && task.Progress.Value != exchangeTask.ProgressPercent))
            {
                // Progress is between 0.0 and 1.0
                task.Progress = exchangeTask.ProgressPercent / 100;
            }

            task.Modified = DateTime.Now;
            // Remove recurring if task is completed
            if (!exchangeTask.IsRecurring || exchangeTask.Completed.HasValue)
            {
                task.FrequencyType = FrequencyType.Once;
            }
            else
            {
                task.UseFixedDate = true;

                switch (exchangeTask.RecurrenceType)
                {
                    case ExchangeRecurrencePattern.None:

                        break;

                    case ExchangeRecurrencePattern.Daily:
                    case ExchangeRecurrencePattern.DailyRegeneration:

                        if (exchangeTask.Interval == 1)
                        {
                            task.CustomFrequency = FrequencyFactory.GetCustomFrequency(FrequencyType.Daily);
                        }
                        else
                        {
                            var frequency = FrequencyFactory.GetCustomFrequency<EveryXPeriodFrequency>(FrequencyType.EveryXPeriod);
                            frequency.Scale = CustomFrequencyScale.Day;
                            frequency.Rate = exchangeTask.Interval;

                            task.CustomFrequency = frequency;
                        }

                        task.UseFixedDate = exchangeTask.RecurrenceType == ExchangeRecurrencePattern.Daily;

                        break;

                    case ExchangeRecurrencePattern.Weekly:
                    case ExchangeRecurrencePattern.WeeklyRegeneration:

                        if (exchangeTask.Interval < 2)
                        {
                            int daysCount = 0;
                            var frequency = FrequencyFactory.GetCustomFrequency<DaysOfWeekFrequency>(FrequencyType.DaysOfWeek);

                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Monday) == ExchangeDayOfWeek.Monday)
                            {
                                daysCount++;
                                frequency.IsMonday = true;
                            }
                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Tuesday) == ExchangeDayOfWeek.Tuesday)
                            {
                                daysCount++;
                                frequency.IsTuesday = true;
                            }
                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Wednesday) == ExchangeDayOfWeek.Wednesday)
                            {
                                daysCount++;
                                frequency.IsWednesday = true;
                            }
                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Thursday) == ExchangeDayOfWeek.Thursday)
                            {
                                daysCount++;
                                frequency.IsThursday = true;
                            }
                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Friday) == ExchangeDayOfWeek.Friday)
                            {
                                daysCount++;
                                frequency.IsFriday = true;
                            }
                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Saturday) == ExchangeDayOfWeek.Saturday)
                            {
                                daysCount++;
                                frequency.IsSaturday = true;
                            }
                            if ((exchangeTask.DaysOfWeek & ExchangeDayOfWeek.Sunday) == ExchangeDayOfWeek.Sunday)
                            {
                                daysCount++;
                                frequency.IsSunday = true;
                            }

                            // if the weekly frequency repeat on a single day that match the due date or the start date
                            // use the WeeklyFrequency instead of DaysOfWeek
                            bool matchDaysOfWeek = (!exchangeTask.Start.HasValue && exchangeTask.Due.HasValue && exchangeTask.DaysOfWeek.ToDayOfWeek() == exchangeTask.Due.Value.DayOfWeek) 
                                || (exchangeTask.Start.HasValue && exchangeTask.DaysOfWeek.ToDayOfWeek() == exchangeTask.Start.Value.DayOfWeek);
                            if (daysCount == 0 || (daysCount == 1 && matchDaysOfWeek))
                            {
                                task.CustomFrequency = FrequencyFactory.GetCustomFrequency(FrequencyType.Weekly);
                            }
                            else if (daysCount > 0)
                            {
                                task.CustomFrequency = frequency;
                            }
                            else
                            {
                                TrackingManagerHelper.Trace("Exchange update task weekly frequency without days");
                            }
                        }
                        else
                        {
                            var frequency = FrequencyFactory.GetCustomFrequency<EveryXPeriodFrequency>(FrequencyType.EveryXPeriod);
                            frequency.Scale = CustomFrequencyScale.Week;
                            frequency.Rate = exchangeTask.Interval;

                            task.CustomFrequency = frequency;
                        }

                        task.UseFixedDate = exchangeTask.RecurrenceType == ExchangeRecurrencePattern.Weekly;

                        break;

                    case ExchangeRecurrencePattern.Monthly:
                    case ExchangeRecurrencePattern.MonthlyRegeneration:

                        if (exchangeTask.Interval < 2)
                        {
                            task.CustomFrequency = FrequencyFactory.GetCustomFrequency(FrequencyType.Monthly);
                        }
                        else
                        {
                            var frequency = FrequencyFactory.GetCustomFrequency<EveryXPeriodFrequency>(FrequencyType.EveryXPeriod);
                            frequency.Scale = CustomFrequencyScale.Month;
                            frequency.Rate = exchangeTask.Interval;

                            task.CustomFrequency = frequency;
                        }

                        task.UseFixedDate = exchangeTask.RecurrenceType == ExchangeRecurrencePattern.Monthly;

                        break;

                    case ExchangeRecurrencePattern.MonthlyRelative:
                        {
                            if (exchangeTask.Interval == 1)
                            {
                                var frequency = FrequencyFactory.GetCustomFrequency<OnXDayFrequency>(FrequencyType.OnXDayOfEachMonth);
                                frequency.RankingPosition = exchangeTask.DayOfWeekIndex.ToRankingPosition();
                                frequency.DayOfWeek = exchangeTask.DaysOfWeek.ToDayOfWeek();

                                task.CustomFrequency = frequency;
                            }
                            else if (exchangeTask.Interval > 1)
                            {
                                if (exchangeTask.Due.HasValue && exchangeTask.Due.Value.DayOfWeek == exchangeTask.DaysOfWeek.ToDayOfWeek())
                                {
                                    // repeat every N month, day of the week match due date => use EveryXPeriod
                                    var frequency = FrequencyFactory.GetCustomFrequency<EveryXPeriodFrequency>(FrequencyType.EveryXPeriod);
                                    frequency.Rate = exchangeTask.Interval;
                                    frequency.Scale = CustomFrequencyScale.Month;

                                    task.CustomFrequency = frequency;
                                }
                            }
                        }

                        break;

                    case ExchangeRecurrencePattern.Yearly:
                    case ExchangeRecurrencePattern.YearlyRegeneration:

                        if (exchangeTask.Interval < 2)
                        {
                            task.CustomFrequency = FrequencyFactory.GetCustomFrequency(FrequencyType.Yearly);
                        }
                        else
                        {
                            var frequency = FrequencyFactory.GetCustomFrequency<EveryXPeriodFrequency>(FrequencyType.EveryXPeriod);
                            frequency.Scale = CustomFrequencyScale.Year;
                            frequency.Rate = exchangeTask.Interval;

                            task.CustomFrequency = frequency;
                        }
                        task.UseFixedDate = exchangeTask.RecurrenceType == ExchangeRecurrencePattern.Yearly;

                        break;

                    default:
                        TrackingManagerHelper.Trace("ExchangeHelper - Unknown recurrence type: " + exchangeTask.RecurrenceType);
                        break;
                }
            }
        }

        public static TaskPriority GetPriority(this ExchangeTaskImportance taskImportance)
        {
            switch (taskImportance)
            {
                case ExchangeTaskImportance.Low:
                    return TaskPriority.Low;
                case ExchangeTaskImportance.Normal:
                    return TaskPriority.Medium;
                case ExchangeTaskImportance.High:
                    return TaskPriority.High;

                default:
                    throw new ArgumentOutOfRangeException("taskImportance");
            }
        }

        public static ExchangeTaskImportance GetImportance(this TaskPriority taskPriority)
        {
            switch (taskPriority)
            {
                case TaskPriority.None:
                case TaskPriority.Low:
                    return ExchangeTaskImportance.Low;
                case TaskPriority.Medium:
                    return ExchangeTaskImportance.Normal;
                case TaskPriority.High:
                case TaskPriority.Star:
                    return ExchangeTaskImportance.High;

                default:
                    throw new ArgumentOutOfRangeException("taskPriority");
            }
        }

        public static ExchangeDayOfWeek ToExchangeDayOfWeek(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return ExchangeDayOfWeek.Monday;
                case DayOfWeek.Tuesday:
                    return ExchangeDayOfWeek.Tuesday;
                case DayOfWeek.Wednesday:
                    return ExchangeDayOfWeek.Wednesday;
                case DayOfWeek.Thursday:
                    return ExchangeDayOfWeek.Thursday;
                case DayOfWeek.Friday:
                    return ExchangeDayOfWeek.Friday;
                case DayOfWeek.Saturday:
                    return ExchangeDayOfWeek.Saturday;
                case DayOfWeek.Sunday:
                    return ExchangeDayOfWeek.Sunday;
            }

            return ExchangeDayOfWeek.None;
        }

        public static DayOfWeek ToDayOfWeek(this ExchangeDayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case ExchangeDayOfWeek.Monday:
                    return DayOfWeek.Monday;
                case ExchangeDayOfWeek.Tuesday:
                    return DayOfWeek.Tuesday;
                case ExchangeDayOfWeek.Wednesday:
                    return DayOfWeek.Wednesday;
                case ExchangeDayOfWeek.Thursday:
                    return DayOfWeek.Thursday;
                case ExchangeDayOfWeek.Friday:
                    return DayOfWeek.Friday;
                case ExchangeDayOfWeek.Saturday:
                    return DayOfWeek.Saturday;
                case ExchangeDayOfWeek.Sunday:
                    return DayOfWeek.Sunday;
            }

            return DayOfWeek.Monday;
        }

        public static ExchangeDayOfWeekIndex ToExchangeDayIndex(this RankingPosition position)
        {
            switch (position)
            {
                case RankingPosition.First:
                    return ExchangeDayOfWeekIndex.First;
                case RankingPosition.Second:
                    return ExchangeDayOfWeekIndex.Second;
                case RankingPosition.Third:
                    return ExchangeDayOfWeekIndex.Third;
                case RankingPosition.Fourth:
                    return ExchangeDayOfWeekIndex.Fourth;
                case RankingPosition.Last:
                    return ExchangeDayOfWeekIndex.Last;
            }

            return ExchangeDayOfWeekIndex.First;
        }

        public static RankingPosition ToRankingPosition(this ExchangeDayOfWeekIndex position)
        {
            switch (position)
            {
                case ExchangeDayOfWeekIndex.First:
                    return RankingPosition.First;
                case ExchangeDayOfWeekIndex.Second:
                    return RankingPosition.Second;
                case ExchangeDayOfWeekIndex.Third:
                    return RankingPosition.Third;
                case ExchangeDayOfWeekIndex.Fourth:
                    return RankingPosition.Fourth;
                case ExchangeDayOfWeekIndex.Last:
                    return RankingPosition.Last;
            }

            return RankingPosition.First;
        }
    }
}
