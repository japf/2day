using System;
using System.Linq;
using Windows.ApplicationModel.VoiceCommands;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Background
{
    internal class HowManyTasksTodayCortanaBackgroundCommand : CortanaBackgroundCommandBase
    {
        public HowManyTasksTodayCortanaBackgroundCommand() : base("howManyTasks")
        {
        }

        public override VoiceCommandResponse Execute(IWorkbook workbook, IPersistenceLayer persistenceLayer, Windows.ApplicationModel.VoiceCommands.VoiceCommand voiceCommand)
        {
            var dateTime = DateTime.Now.Date;
            int count = workbook.Tasks.Count(t => (t.Due.HasValue && t.Due.Value.Date == dateTime) || t.IsLate);

            var userMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = "Aujourd'hui, vous avez " + count + " tâches",
                SpokenMessage = "Aujourd'hui, vous avez " + count + " tâches"
            };

            return VoiceCommandResponse.CreateResponse(userMessage);
        }
    }
}