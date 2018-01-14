using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace Chartreuse.Today.App.VoiceCommand
{
    internal static class CortanaDialogHelper
    {
        /// <summary>
        /// Show a progress screen. These should be posted at least every 5 seconds for a 
        /// long-running operation, such as accessing network resources over a mobile 
        /// carrier network.
        /// </summary>
        /// <param name="voiceCommandServiceConnection"></param>
        /// <param name="message">The message to display, relating to the task being performed.</param>
        internal static async Task ShowProgressScreen(VoiceCommandServiceConnection voiceCommandServiceConnection, string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = message,
                SpokenMessage = message
            };

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);

            await voiceCommandServiceConnection.ReportProgressAsync(response);
        }
    }
}