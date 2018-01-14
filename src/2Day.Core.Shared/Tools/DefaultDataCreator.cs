using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public static class DefaultDataCreator
    {
        public static int ViewCount = 12;

        public static void SetupDefaultOptions(IWorkbook workbook)
        {
            workbook.Settings.SetValue(CoreSettings.ShowFutureStartDates, true);
        }

        public static void CreateViews(IWorkbook workbook)
        {
            var views = new List<SystemView>();

            views.Add(new SystemView
            {
                ViewKind = ViewKind.Today,
                Name = StringResources.SystemView_TitleToday, IconId = FontIconHelper.FolderTodayIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 0,
                IsEnabled = true
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.Tomorrow,
                Name = StringResources.SystemView_TitleTomorrow,
                IconId = FontIconHelper.FolderTomorrowIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 1,
                IsEnabled = false
            });
            
            views.Add(new SystemView
            {
                ViewKind = ViewKind.Week,
                Name = StringResources.SystemView_TitleWeek,
                IconId = FontIconHelper.FolderNextWeekIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 2,
                IsEnabled = true
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.All,
                Name = StringResources.SystemView_TitleAll,
                IconId = FontIconHelper.FolderAllIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 3,
                IsEnabled = true
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.Completed,
                Name = StringResources.SystemView_TitleCompleted,
                IconId = FontIconHelper.FolderCompletedIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 4,
                IsEnabled = true
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.Starred,
                Name = StringResources.SystemView_TitleStarred,
                IconId = FontIconHelper.FolderStarredIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 5,
                IsEnabled = true
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.NoDate,
                Name = StringResources.SystemView_TitleNoDate,
                IconId = FontIconHelper.FolderNoDateIconId,
                TaskGroup = TaskGroup.Folder,
                GroupAscending = true,
                Order = 6,
                IsEnabled = true
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.Late,
                Name = StringResources.SystemView_TitleLate,
                IconId = FontIconHelper.FolderLateIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 7,
                IsEnabled = false
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.StartDate,
                Name = StringResources.SystemView_TitleStartDate,
                IconId = FontIconHelper.FolderStartDateIconId,
                TaskGroup = TaskGroup.StartDate,
                GroupAscending = true,
                Order = 8,
                IsEnabled = false
            });
            
            views.Add(new SystemView
            {
                ViewKind = ViewKind.Reminder,
                Name = StringResources.SystemView_TitleReminder,
                IconId = FontIconHelper.FolderReminderIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 9,
                IsEnabled = false
            });
            
            views.Add(new SystemView
            {
                ViewKind = ViewKind.NonCompleted,
                Name = StringResources.SystemView_TitleNonCompleted,
                IconId = FontIconHelper.FolderNonCompletedIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 10,
                IsEnabled = false
            });

            views.Add(new SystemView
            {
                ViewKind = ViewKind.ToSync,
                Name = StringResources.SystemView_TitleToSync,
                IconId = FontIconHelper.FolderToSyncIconId,
                TaskGroup = TaskGroup.DueDate,
                GroupAscending = true,
                Order = 11,
                IsEnabled = false
            });

            foreach (var systemView in views)
                workbook.AddView(systemView);            
        }

        public static void CreateFolders(IWorkbook workbook)
        {
            var personalFolder = workbook.AddFolder(StringResources.Startup_DefaultFolderName_Personal);
            personalFolder.Color = ColorChooser.Blue;
            personalFolder.IconId = 10;

            var workFolder = workbook.AddFolder(StringResources.Startup_DefaultFolderName_Work);
            workFolder.Color = ColorChooser.Green;
            workFolder.IconId = 2;
        }

        public static void CreateDefaultTasks(IWorkbook workbook, DeviceFamily deviceFamily)
        {
            var welcomeTask = new Task
            {
                Title = StringResources.BootstrapWelcomeTaskTitle,
                Added = DateTime.Now,
                Due = DateTime.Today,
                Note = StringResources.BootstrapWelcomeTaskNote,
                Priority = TaskPriority.High,
                Modified = DateTime.Now,
                Folder = workbook.Folders[0]
            };


            if (deviceFamily == DeviceFamily.WindowsDesktop)
            {
                var dragTask = new Task
                {
                    Title = StringResources.BootstrapDragTaskTitle,
                    Added = DateTime.Now,
                    Due = DateTime.Today.AddDays(1.0),
                    Note = StringResources.BootstrapDragTaskNote,
                    Priority = TaskPriority.Medium,
                    Modified = DateTime.Now,
                    Folder = workbook.Folders[0]
                };
            }
            else if (deviceFamily == DeviceFamily.WindowsMobile)
            {
                var slideTask = new Task
                {
                    Title = StringResources.BootstrapSlideTaskTitle,
                    Added = DateTime.Now,
                    Due = DateTime.Today.AddDays(1.0),
                    Note = StringResources.BootstrapSlideTaskNote,
                    Priority = TaskPriority.Medium,
                    Modified = DateTime.Now,
                    Folder = workbook.Folders[0]
                };
            }
        }
    }
}
