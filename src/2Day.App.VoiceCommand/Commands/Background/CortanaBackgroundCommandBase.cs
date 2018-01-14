using System.Threading;
using Windows.ApplicationModel.VoiceCommands;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Universal.Manager;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Background
{
    internal abstract class CortanaBackgroundCommandBase : CortanaCommandBase
    {
        private static readonly EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, Constants.SyncPrimitiveBackgroundEvent);

        public abstract VoiceCommandResponse Execute(IWorkbook workbook, IPersistenceLayer persistenceLayer, Windows.ApplicationModel.VoiceCommands.VoiceCommand voiceCommand);

        protected CortanaBackgroundCommandBase(string commandName) : base(commandName)
        {
        }

        protected string ExtractPropertyFromVoiceCommand(Windows.ApplicationModel.VoiceCommands.VoiceCommand voiceCommand, string propertyKey)
        {
            string result = string.Empty;

            if (voiceCommand.Properties.ContainsKey(propertyKey))
            {
                var entries = voiceCommand.Properties[propertyKey];

                if ((entries != null) && (entries.Count > 0))
                {
                    result = entries[0];
                }
            }
            return (result);
        }

        protected void UpdateLiveTiles(IWorkbook workbook)
        {
            var tileManager = new TileManager(workbook, new TrackingManager(false, DeviceFamily.Unkown), null, true);
            tileManager.LoadSecondaryTilesAsync().Wait(500);
            tileManager.UpdateTiles();
        }

        protected void SignalForegroundAppWorkbookChanged()
        {
            waitHandle.Set();
        }
    }
}
