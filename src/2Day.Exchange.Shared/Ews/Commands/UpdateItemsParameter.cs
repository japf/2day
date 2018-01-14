using System;
using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Ews.Schema;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class UpdateItemsParameter : EwsRequestParameterBuilderBase
    {
        public List<EwsTask> Tasks { get; private set; }

        public UpdateItemsParameter()
        {
            this.Tasks = new List<EwsTask>();
        }

        protected override void EnsureCanExecute()
        {            
        }

        protected override Dictionary<string, string> GetCommandAttributes()
        {
            return new Dictionary<string, string>
            {
                { "ConflictResolution", "AutoResolve" },
                { "MessageDisposition", "SaveOnly" }
            };
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <m:ItemChanges>" +
                "        {0}" +
                "   </m:ItemChanges>";

            const string itemTemplate =
                "<t:ItemChange>" +
                "   <t:ItemId Id='{0}' ChangeKey='{1}'/>" +
                "   <t:Updates>" +
                "       {2} " +
                "   </t:Updates>" +
                "</t:ItemChange>";

            var builder = new StringBuilder();

            foreach (var task in this.Tasks)
            {
                if (task.Changes == EwsFields.None)
                {
                    LogService.Log("EwsSyncService", $"Task {task.Id} {task.Subject} have changes equals to EwsFields.None");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(task.Id))
                {
                    LogService.Log("EwsSyncService", $"Task {task.Subject} have no id");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(task.ChangeKey))
                {
                    LogService.Log("EwsSyncService", $"Task {task.Id} {task.Subject} have no change key");
                    continue;
                }

                string itemXml = this.GetTaskXmlChanges(task);

                builder.AppendLine(string.Format(itemTemplate, task.Id, task.ChangeKey, itemXml));
            }

            var content = string.Format(command, builder);

            return content;
        }

        private string GetTaskXmlChanges(EwsTask task)
        {
            var builder = new StringBuilder();

            if (task.Changes.HasFlag(EwsFields.Body) && task.Type == EwsItemType.Task)
            {
                if (!string.IsNullOrEmpty(task.Body))
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.Body, task.Body, new EwsAttributes { { "BodyType", task.BodyType.ToString() } }));
                else
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.Body));
            }

            if (task.Changes.HasFlag(EwsFields.Categories))
            {
                if (task.Categories != null)
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.Categories, task.Categories));
                else
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.Categories, new string[0]));
            }

            if (task.Changes.HasFlag(EwsFields.Importance))
            {
                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.Importance, task.Importance));
            }

            if (task.Changes.HasFlag(EwsFields.Subject))
            {
                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagSubject, task.Subject));
                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagSubjectPrefix, string.Empty));
                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.NormalizedSubject, task.Subject));
                
                if (task.Type == EwsItemType.Item)
                {
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskTitle, task.Subject));
                }
            }

            if (task.Changes.HasFlag(EwsFields.StartDate))
            {
                if (task.StartDate.HasValue)
                {
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.CommonStart, task.StartDate.Value, DateTimeKind.Utc));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskStartDate, task.StartDate.Value, DateTimeKind.Local));
                }
                else
                {
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.CommonStart));
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.TaskStartDate));
                }
            }

            if (task.Changes.HasFlag(EwsFields.DueDate))
            {
                if (task.DueDate.HasValue)
                {
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.CommonEnd, task.DueDate.Value, DateTimeKind.Utc));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskDueDate, task.DueDate.Value, DateTimeKind.Local));
                }
                else
                {
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.CommonEnd));
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.TaskDueDate));
                }
            }

            bool hasSetPercentComplete = false;

            if (task.Type == EwsItemType.Task)
            {
                if (task.Changes.HasFlag(EwsFields.CompleteDate))
                {
                    if (task.CompleteDate.HasValue)
                    {
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.FlagRequest, string.Empty));
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskCompletedDate, task.CompleteDate.Value, DateTimeKind.Utc));
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskComplete, true));
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskStatus, EwsTaskStatus.Completed));
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskPercentComplete, 1.0));

                        hasSetPercentComplete = true;
                    }
                    else
                    {
                        builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.TaskCompletedDate));
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskComplete, false));
                        builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskStatus, EwsTaskStatus.NotStarted));
                    }
                }
            }
            else
            {
                if (task.CompleteDate.HasValue)
                {
                    //string str = task.CompleteDate.ToExchangeString(DateTimeKind.Utc);
                    //TagFlagStatus "Integer", 4240, (string)null, (object)MapiConstants.PidLidTaskStatusEnum.Completed));
                    //TagFlagCompleteTime "SystemTime", 4241, (string)null, (object)str));
                    //ReplyRequested "Boolean", 3095, (string)null, (object)false));
                    //ResponseRequested "Boolean", 99, (string)null, (object)false));
                    //TagToDoItemFlags "Integer", 3627, (string)null, (object)1));
                    //TaskCompletedDate "SystemTime", 33039, "00062003-0000-0000-C000-000000000046", (object)str));
                    //TaskComplete "Boolean", 33052, "00062003-0000-0000-C000-000000000046", (object)true));
                    //TaskStatus "Integer", 33025, "00062003-0000-0000-C000-000000000046", (object)MapiConstants.PidLidTaskStatusEnum.Completed));
                    //TaskPercentComplete "Double", 33026, "00062003-0000-0000-C000-000000000046", (object)1f));
                    //ReminderSet "Boolean", 34051, "00062008-0000-0000-C000-000000000046", (object)false));
                    //TagFollowupIcon "Integer", 4245, (string)null));

                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagFlagStatus, (int)PidLidTaskStatusEnum.Completed));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagFlagCompleteTime, task.CompleteDate.Value, DateTimeKind.Utc));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ReplyRequested, false));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ResponseRequested, false));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagToDoItemFlags, 1));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskCompletedDate, task.CompleteDate.Value, DateTimeKind.Utc));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskComplete, true));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskStatus, (int)PidLidTaskStatusEnum.Completed));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskPercentComplete, 1.0));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ReminderSet, false));
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.TagFollowupIcon));
                }
                else
                {
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagFlagStatus, (int)PidTagTaskStatusEnum.Flagged));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ReplyRequested, true));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ResponseRequested, true));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagToDoItemFlags, 1));
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.TagFlagCompleteTime));
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.TaskCompletedDate));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskComplete, false));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskStatus, (int)PidLidTaskStatusEnum.InProgress));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskPercentComplete, 0.0));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ReminderSet, false));
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagFollowupIcon, (int)PidTagFollowupIconEnum.Followup));

                }
            }

            if (task.Changes.HasFlag(EwsFields.PercentComplete) && !hasSetPercentComplete)
            {
                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskPercentComplete, task.PercentComplete));
            }

            if (task.Changes.HasFlag(EwsFields.Recurrence) && task.Type == EwsItemType.Task)
            {
                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskIsRecurring, task.IsRecurring));

                if (task.Recurrence != null)
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldRawValueXml(EwsFieldUri.Recurrence, task.Recurrence.BuildXml()));
                else
                    builder.AppendLine(EwsTaskSchema.BuildClearFieldValueXml(EwsFieldUri.Recurrence));

                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TaskIsDeadOccurence, task.IsDeadOccurence));

                if (task.IsRecurring && !task.IsDeadOccurence)
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagIconIndex, (int)EwsTagIconIndex.UnassignedRecurringTask));
                else
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.TagIconIndex, (int)EwsTagIconIndex.Task));
            }

            if (task.Changes.HasFlag(EwsFields.Reminder))
            {
                if (task.ReminderDate.HasValue)
                    builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ReminderDueBy, task.ReminderDate.Value, DateTimeKind.Utc));
                // note: delete is not supported for the ReminderDueBy field

                builder.AppendLine(EwsTaskSchema.BuildUpdateFieldValueXml(EwsFieldUri.ReminderIsSet, task.ReminderIsSet, false));
            }

            var content = builder.ToString();

            return content;
        }
    }
}