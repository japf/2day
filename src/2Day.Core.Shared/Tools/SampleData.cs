using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public static class SampleData
    {
        public static void AddBigSampleData(IWorkbook workbook)
        {
            workbook.RemoveAll();

            var personalFolder = workbook.AddFolder("personal");
            personalFolder.Color = ColorChooser.Blue;
            personalFolder.IconId = 10;

            var workFolder = workbook.AddFolder("work");
            workFolder.Color = ColorChooser.Green;
            workFolder.IconId = 2;

            var homeFolder = workbook.AddFolder("home");
            homeFolder.Color = ColorChooser.Orange;
            homeFolder.IconId = 2;

            workbook.AddContext("@home");
            workbook.AddContext("@work");
            workbook.AddContext("@phone");
            workbook.AddContext("@mall");

            int taskCount = 1;
            var rand = new Random((int)DateTime.Now.Ticks);
            foreach (var folder in new[] { personalFolder, workFolder, homeFolder })
            {
                var date = DateTime.Now.AddDays(-2);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < rand.Next(5, 8); j++)
                    {
                        var task = new Task
                        {
                            Title = taskCount.ToString(),
                            Added = DateTime.Now,
                            Due = date,
                            Priority = (TaskPriority) rand.Next(5),
                            Note = $"Note for task {folder.Name} {j}"
                        };

                        if (rand.Next(4) == 1)
                        {
                            task.Start = task.Due.Value.AddDays(-3);
                        }

                        taskCount++;

                        // set progress
                        double progress = rand.Next(10); // 0 -> 9
                        if (progress > 0)
                            task.Progress = progress / 10;

                        task.Folder = folder;

                        // set context 
                        var context = rand.Next(workbook.Contexts.Count);
                        if (context < workbook.Contexts.Count - 1)
                            task.Context = workbook.Contexts[context];
                    }

                    date = date.AddDays(1);
                }
            }
        }

        public static void AddRealSampleData(IWorkbook workbook)
        {
            workbook.RemoveAll();

            var personalFolder = workbook.AddFolder(StringResources.Startup_DefaultFolderName_Personal);
            personalFolder.Color = ColorChooser.Blue;
            personalFolder.IconId = 10;
            personalFolder.Order = 1;

            var task1 = new Task
            {
                Title = StringResources.SampleData_FolderPersonal_Task1,
                Added = DateTime.Now,
                Due = DateTime.Now,
                Note = StringResources.SampleData_FolderPersonal_Task1_Note,
                Priority = TaskPriority.Star,
                Folder = personalFolder,
                Action = TaskAction.Call
            };

            var task2 = new Task
            {
                Title = StringResources.SampleData_FolderPersonal_Task2,
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(2),
                Priority = TaskPriority.High,
                Folder = personalFolder
            };

            var task3 = new Task
            {
                Title = StringResources.SampleData_FolderPersonal_Task3,
                Note = "52$",
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(1),
                Priority = TaskPriority.High,
                Folder = personalFolder
            };

            var workFolder = workbook.AddFolder(StringResources.Startup_DefaultFolderName_Work);
            workFolder.Color = ColorChooser.Green;
            workFolder.IconId = 2;
            workFolder.Order = 2;

            var task4 = new Task
            {
                Title = StringResources.SampleData_FolderWork_Task1,
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(1),
                Priority = TaskPriority.Star,
                Folder = workFolder,
                Action = TaskAction.Email
            };

            var task5 = new Task
            {
                Title = StringResources.SampleData_FolderWork_Task2,
                Added = DateTime.Now,
                Due = DateTime.Now,
                Note = StringResources.SampleData_FolderWork_Task2_Description,
                Priority = TaskPriority.High,
                Folder = workFolder
            };

            var task6 = new Task
            {
                Title = StringResources.SampleData_FolderWork_Task3,
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(4),
                Priority = TaskPriority.Medium,
                Folder = workFolder,
                Action = TaskAction.Visit
            };

            var task7 = new Task
            {
                Title = StringResources.SampleData_FolderWork_Task4,
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(6),
                Priority = TaskPriority.High,
                Folder = workFolder
            };

            var shoppingFolder = workbook.AddFolder(StringResources.SampleData_FolderShopping);
            shoppingFolder.Color = ColorChooser.Orange;
            shoppingFolder.IconId = 28;
            shoppingFolder.Order = 3;

            var task8 = new Task
            {
                Title = StringResources.SampleData_FolderShopping_Task1,
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(2),
                Priority = TaskPriority.Star,
                Folder = shoppingFolder
            };

            var task9 = new Task
            {
                Title = StringResources.SampleData_FolderShopping_Task2,
                Added = DateTime.Now,
                Due = null,
                Priority = TaskPriority.Medium,
                Folder = shoppingFolder
            };

            var hobbiesFolder = workbook.AddFolder(StringResources.SampleData_FolderHobbies);
            hobbiesFolder.Color = ColorChooser.Purple;
            hobbiesFolder.IconId = 17;
            hobbiesFolder.Order = 4;

            var task10 = new Task
            {
                Title = StringResources.SampleData_FolderHobbies_Task1,
                Added = DateTime.Now,
                Due = DateTime.Now,
                Priority = TaskPriority.Star,
                Folder = hobbiesFolder,
                Action = TaskAction.Call
            };

            var task11 = new Task
            {
                Title = StringResources.SampleData_FolderHobbies_Task2,
                Added = DateTime.Now,
                Due = null,
                Note = "Fuji",
                Priority = TaskPriority.Medium,
                Folder = hobbiesFolder,
                Action = TaskAction.Visit
            };

            var task12 = new Task
            {
                Title = StringResources.SampleData_FolderHobbies_Task3,
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(3),
                Priority = TaskPriority.High,
                Folder = hobbiesFolder
            };

            var moviesFolder = workbook.AddFolder(StringResources.SampleDate_FolderMovies);
            moviesFolder.Color = ColorChooser.Yellow;
            moviesFolder.IconId = 30;
            moviesFolder.Order = 5;

            var task13 = new Task
            {
                Title = "The Artist",
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(1),
                Priority = TaskPriority.High,
                Folder = moviesFolder
            };

            var task14 = new Task
            {
                Title = "Sherlock Holmes",
                Added = DateTime.Now,
                Due = DateTime.Now.AddDays(10),
                Priority = TaskPriority.Medium,
                Folder = moviesFolder
            };

            var task15 = new Task
            {
                Title = "Inception",
                Added = DateTime.Now,
                Due = null,
                Priority = TaskPriority.Low,
                Folder = moviesFolder
            };

            workbook.AddContext(StringResources.Startup_DefaultContextHome);
            workbook.AddContext(StringResources.Startup_DefaultContextPhone);
            workbook.AddContext(StringResources.Startup_DefaultContextQuick);
        }
    }
}
