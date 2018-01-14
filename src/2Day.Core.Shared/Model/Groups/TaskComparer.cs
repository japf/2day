using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Groups
{
    public class TaskComparer : IComparer<ITask>
    {
        private readonly Func<ITask, ITask, int> compare1;
        private readonly Func<ITask, ITask, int> compare2;
        private readonly Func<ITask, ITask, int> compare3;

        public TaskComparer(ISettings settings)
        {
            var order1 = settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType1);
            var order2 = settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType2);
            var order3 = settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType3);

            this.compare1 = CreateCompareFunc(order1, settings.GetValue<bool>(CoreSettings.TaskOrderingAscending1));
            if (order2 != order1)
            {
                this.compare2 = CreateCompareFunc(order2, settings.GetValue<bool>(CoreSettings.TaskOrderingAscending2));
                if (order3 != order2 && order3 != order1)
                {
                    this.compare3 = CreateCompareFunc(order3, settings.GetValue<bool>(CoreSettings.TaskOrderingAscending3));
                }
            }
        }

        private static Func<ITask, ITask, int> CreateCompareFunc(TaskOrdering ordering, bool ascending)
        {
            switch (ordering)
            {
                case TaskOrdering.AddedDate:
                    return (x, y) => ascending ? x.Added.CompareTo(y.Added) : y.Added.CompareTo(x.Added);
                case TaskOrdering.ModifiedDate:
                    return (x, y) => ascending ? x.Modified.CompareTo(y.Modified) : y.Modified.CompareTo(x.Modified);
                case TaskOrdering.DueDate:
                    return (x, y) =>
                    {
                        if (x.Due.HasValue && y.Due.HasValue)
                        {
                            // both have due date
                            if (x.Due.Value.Date < y.Due.Value.Date)
                                return ascending ? -1 : 1;
                            else if (x.Due.Value.Date > y.Due.Value.Date)
                                return ascending ? 1 : -1;
                            else
                                return 0;
                        }
                        else if (x.Due.HasValue && !y.Due.HasValue)
                        {
                            // only x has due date
                            return ascending ? -1 : 1;
                        }
                        else if (!x.Due.HasValue && y.Due.HasValue)
                        {
                            // only y has due date
                            return ascending ? 1 : -1;
                        }
                        else
                        {
                            return 0;
                        }
                    };
                case TaskOrdering.StartDate:
                    return (x, y) =>
                    {
                        if (x.Start.HasValue && y.Start.HasValue)
                        {
                            // both have start date
                            if (x.Start.Value < y.Start.Value)
                                return ascending ? -1 : 1;
                            else if (x.Start.Value > y.Start.Value)
                                return ascending ? 1 : -1;
                            else
                                return 0;
                        }
                        else if (x.Start.HasValue && !y.Start.HasValue)
                        {
                            // only x has start date
                            return -1;
                        }
                        else if (!x.Start.HasValue && y.Start.HasValue)
                        {
                            // only y has start date
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    };
                case TaskOrdering.Alarm:
                    return (x, y) =>
                    {
                        if (x.Alarm.HasValue && y.Alarm.HasValue)
                        {
                            // both have alarm
                            if (x.Alarm.Value < y.Alarm.Value)
                                return ascending ? -1 : 1;
                            else if (x.Alarm.Value > y.Alarm.Value)
                                return ascending ? 1 : -1;
                            else
                                return 0;
                        }
                        else if (x.Alarm.HasValue && !y.Alarm.HasValue)
                        {
                            // only x has alarm
                            return -1;
                        }
                        else if (!x.Alarm.HasValue && y.Alarm.HasValue)
                        {
                            // only y has alarm
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    };
                case TaskOrdering.Context:
                    return (x, y) =>
                    {
                        if (x.Context != null && y.Context != null)
                        {
                            // both have context
                            return ascending ? x.Context.Name.CompareTo(y.Context.Name) : y.Context.Name.CompareTo(x.Context.Name);
                        }
                        else if (x.Context != null && y.Context == null)
                        {
                            // only x has context
                            return ascending ? -1 : 1;
                        }
                        else if (x.Context == null && y.Context != null)
                        {
                            // only y context
                            return ascending ? 1 : -1;
                        }
                        else
                        {
                            return 0;
                        }
                    };
                case TaskOrdering.Folder:
                    return (x, y) =>
                    {
                        if (x.Folder.Name == null && y.Folder.Name != null)
                            return 1;
                        else if (x.Folder.Name != null && y.Folder.Name == null)
                            return -1;
                        else if (x.Folder.Name == null && y.Folder.Name == null)
                            return 0;
                        else
                            return ascending ? x.Folder.Name.CompareTo(y.Folder.Name) : y.Folder.Name.CompareTo(x.Folder.Name);
                    };
                case TaskOrdering.Priority:
                    return (x, y) =>
                    {
                        return ascending ? x.Priority.CompareTo(y.Priority) : y.Priority.CompareTo(x.Priority);
                    };
                case TaskOrdering.Alphabetical:
                    return (x, y) =>
                    {
                        if (x.Title == null && y.Title != null)
                            return 1;
                        else if (x.Title != null && y.Title == null)
                            return -1;
                        else if (x.Title == null && y.Title == null)
                            return 0;
                        else
                            return ascending ? x.Title.CompareTo(y.Title) : y.Title.CompareTo(x.Title);
                    };
                default:
                    throw new ArgumentOutOfRangeException("ordering");
            }
        }

        public int Compare(ITask x, ITask y)
        {
            if (x == y || (x == null && y == null))
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return 1;
            }
            else if (x != null && y == null)
            {
                return -1;
            }
            else
            {
                int c1 = this.compare1(x, y);
                if (c1 != 0)
                    return c1;

                if (this.compare2 != null)
                {
                    int c2 = this.compare2(x, y);
                    if (c2 != 0)
                        return c2;

                    if (this.compare3 != null)
                        return this.compare3(x, y);
                }

                if (x.Added < y.Added)
                    return -1;
                else if (x.Added > y.Added)
                    return 1;
                else if (x.GetHashCode() < y.GetHashCode())
                    return -1;
                else if (x.GetHashCode() > y.GetHashCode())
                    return 1;
                else
                    // we should never reach this point !
                    return 0;
            }
        }
    }
}