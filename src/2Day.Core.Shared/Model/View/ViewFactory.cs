using System;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public static class ViewFactory
    {
        public static ISystemView BuildView(IWorkbook workbook, ISystemView view)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            if (view == null)
                throw new ArgumentNullException("view");

            switch (view.ViewKind)
            {
                case ViewKind.Today:
                    return new ViewToday(workbook, view);
                case ViewKind.Week:
                    return new ViewWeek(workbook, view);
                case ViewKind.All:
                    return new ViewAll(workbook, view);
                case ViewKind.Completed:
                    return new ViewCompleted(workbook, view);
                case ViewKind.Starred:
                    return new ViewStarred(workbook, view);
                case ViewKind.NoDate:
                    return new ViewNoDate(workbook, view);
                case ViewKind.Tomorrow:
                    return new ViewTomorrow(workbook, view);
                case ViewKind.Reminder:
                    return new ViewReminder(workbook, view);
                case ViewKind.Late:
                    return new ViewLate(workbook, view);
                case ViewKind.StartDate:
                    return new ViewStartDate(workbook, view);
                case ViewKind.NonCompleted:
                    return new ViewNonCompleted(workbook, view);
                case ViewKind.ToSync:
                    return new ViewToSync(workbook, view);
                default:
                    throw new ArgumentOutOfRangeException(nameof(view));
            }
        }
    }
}