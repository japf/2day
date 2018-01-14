using System;
using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Ews.Schema;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateItemsParameter : EwsRequestParameterBuilderBase
    {
        private const string BodyTypeAttribute = "BodyType";

        public List<EwsTask> Tasks { get; private set; }

        public EwsFolderIdentifier ParentFolder { get; private set; }

        public CreateItemsParameter(IEnumerable<EwsTask> tasks, EwsFolderIdentifier parentFolder)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            if (parentFolder == null)
                throw new ArgumentNullException("parentFolder");

            this.Tasks = new List<EwsTask>(tasks);
            this.ParentFolder = parentFolder;
        }

        protected override void EnsureCanExecute()
        {
            if (this.Tasks.Count == 0)
                throw new CommandCannotExecuteException("No tasks specified");
            if (this.ParentFolder == null)
                throw new CommandCannotExecuteException("Parent folder is null");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <SavedItemFolderId>" +
                "      {0}" +
                "   </SavedItemFolderId>" +
                "   <Items>" +
                "      {1}" +
                "   </Items>";

            var content = string.Format(command, this.ParentFolder.GetXml(), this.GetXmlForTasks());

            return content;
        }

        private string GetXmlForTasks()
        {
            var builder = new StringBuilder();
            foreach (var task in this.Tasks)
            {
                builder.AppendLine(GetXmlForTask(task));
            }

            return builder.ToString();
        }

        // field ordering available at: http://msdn.microsoft.com/en-us/library/aa563930(v=EXCHG.140).aspx
        private static string GetXmlForTask(EwsTask task)
        {
            const string template = "<t:Task>{0}</t:Task>";

            var builder = new StringBuilder();

            builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Subject, task.Subject));

            if (!string.IsNullOrEmpty(task.Body))
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Body, task.Body, new EwsAttributes { { BodyTypeAttribute, task.BodyType.ToString() } }));

            if (task.Categories != null && task.Categories.Length > 0)
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Categories, task.Categories));

            builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Importance, task.Importance));

            if (task.ReminderDate != null)
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.ReminderDueBy, task.ReminderDate.Value, DateTimeKind.Utc));
            builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.ReminderIsSet, task.ReminderIsSet));

            if (task.OrdinalDate != null)
            {
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TagSubject, task.Subject));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TagSubjectPrefix, string.Empty));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.NormalizedSubject, task.Subject));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskOrdinalDate, task.OrdinalDate.Value, DateTimeKind.Utc));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskSubOrdinalDate, task.SubOrdinalDate));
            }

            if (task.StartDate != null)
            {
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.CommonStart, task.StartDate.Value, DateTimeKind.Utc));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskStartDate, task.StartDate.Value, DateTimeKind.Local));
            }

            if (task.DueDate != null)
            {
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.CommonEnd, task.DueDate.Value, DateTimeKind.Utc));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskDueDate, task.DueDate.Value, DateTimeKind.Local));
            }

            builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskIsRecurring, task.IsRecurring));
            builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskIsDeadOccurence, task.IsDeadOccurence));
            if (task.IsRecurring && !task.IsDeadOccurence)
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TagIconIndex, (int)EwsTagIconIndex.UnassignedRecurringTask));
            else
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TagIconIndex, (int)EwsTagIconIndex.Task));

            if (task.CompleteDate != null)
            {
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskPercentComplete, 1.0));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskComplete, true));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskCompletedDate, task.CompleteDate.Value, DateTimeKind.Utc));
                builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskStatus, EwsTaskStatus.Completed));
            }
            else
            {
                if (task.Recurrence != null)
                    builder.AppendLine(EwsTaskSchema.BuildSetFieldRawValueXml(EwsFieldUri.Recurrence, task.Recurrence.BuildXml()));

                if (task.PercentComplete > 0)
                {
                    builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskPercentComplete, task.PercentComplete));
                    builder.AppendLine(EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.TaskStatus, EwsTaskStatus.InProgress));
                }
            }

            string xml = string.Format(template, builder.ToString());

            return xml;
        }
    }
}