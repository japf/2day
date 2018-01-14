using System;
using Windows.ApplicationModel.VoiceCommands;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Background
{
    internal class AddTaskCortanaBackgroundCommand : CortanaBackgroundCommandBase
    {
        public AddTaskCortanaBackgroundCommand() : base("addTask")
        {
        }

        public override VoiceCommandResponse Execute(IWorkbook workbook, IPersistenceLayer persistenceLayer, Windows.ApplicationModel.VoiceCommands.VoiceCommand voiceCommand)
        {
            string content = this.ExtractPropertyFromVoiceCommand(voiceCommand, "speech");

            content = content.FirstLetterToUpper();
            content = content.Trim();
            content = content.Trim('.');

            var task = new Task
            {
                Added = DateTime.Now,
                Modified = DateTime.Now,
                Title = content,
                Folder = workbook.Folders[0]
            };

            task.Priority = workbook.Settings.GetValue<TaskPriority>(CoreSettings.DefaultPriority);
            task.Due = ModelHelper.GetDefaultDueDate(workbook.Settings);
            task.Start = ModelHelper.GetDefaultStartDate(workbook.Settings);
            task.Context = ModelHelper.GetDefaultContext(workbook);

            persistenceLayer.Save();

            this.UpdateLiveTiles(workbook);
            
            var userMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = string.Format(StringResources.Notification_NewTaskCreatedNoDueDateFormat, content),
                SpokenMessage = StringResources.Notification_NewTaskCreatedNoDueDateFormat.Replace("\"{0}\"", string.Empty)
            };

            this.SignalForegroundAppWorkbookChanged();

            return VoiceCommandResponse.CreateResponse(userMessage);
        }
    }
}