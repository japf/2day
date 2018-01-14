using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsNonRegressionTest : EwsBasicTest
    {
        [TestMethod]
        public async Task Due_date_and_start_date()
        {
            var content = await this.server.EnumerateFolderContentAsync(this.folderIdentifiers.TaskFolderIdentifier);
            if (content.Any())
                await this.server.DeleteItemsAsync(content);

            var task = CreateSampleEwsTask(true);
            task.DueDate = new DateTime(2014, 11, 7);
            task.StartDate = new DateTime(2014, 11, 3);
            /*task.Recurrence = new EwsRecurrence
            {
                RecurrenceType = ExchangeRecurrencePattern.Monthly,
                Interval = 1,
                DayOfMonth = 7,
                StartDate = task.DueDate.Value
            };*/

            await this.server.CreateItemAsync(task);

            var newTask = await this.server.GetTask(task.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            AssertEx.DateAreEquals(task.DueDate, newTask.DueDate);
            AssertEx.DateAreEquals(task.StartDate, newTask.StartDate);
            //Assert.AreEqual(task.Recurrence, newTask.Recurrence);

        }
    }
}
