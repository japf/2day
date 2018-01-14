using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.System.Profile;
using Chartreuse.Today.App.VoiceCommand.Commands.Background;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Manager;

namespace Chartreuse.Today.App.VoiceCommand
{
    public class CortanaBackgroundService
    {
        private readonly AppServiceTriggerDetails triggerDetails;
        private readonly BackgroundTaskDeferral serviceDeferral;
        private readonly List<CortanaBackgroundCommandBase> commands;
        private VoiceCommandServiceConnection voiceServiceConnection;
        private IPersistenceLayer persistenceLayer;
        private TrackingManager trackingManager;

        public CortanaBackgroundService(AppServiceTriggerDetails triggerDetails, BackgroundTaskDeferral serviceDeferral)
        {
            if (serviceDeferral == null)
                throw new ArgumentNullException(nameof(serviceDeferral));

            this.triggerDetails = triggerDetails;
            this.serviceDeferral = serviceDeferral;

            this.commands = new List<CortanaBackgroundCommandBase>
            {
                new HowManyTasksTodayCortanaBackgroundCommand(),
                new AddTaskCortanaBackgroundCommand()
            };
        }

        public async Task HandleCommandAsync()
        {
            // This should match the uap:AppService and VoiceCommandService references from the 
            // package manifest and VCD files, respectively. Make sure we've been launched by
            // a Cortana Voice Command.
            if (this.triggerDetails != null && this.triggerDetails.Name == CortanaConstants.BackgroundServiceName)
            {
                try
                {
                    DeviceFamily deviceFamily = DeviceFamily.Unkown;
                    if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop", StringComparison.OrdinalIgnoreCase))
                        deviceFamily = DeviceFamily.WindowsDesktop;
                    else if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.OrdinalIgnoreCase))
                        deviceFamily = DeviceFamily.WindowsMobile;

                    this.trackingManager = new TrackingManager(false, deviceFamily);

                    this.voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(this.triggerDetails);
                    this.voiceServiceConnection.VoiceCommandCompleted += this.OnVoiceCommandCompleted;

                    Windows.ApplicationModel.VoiceCommands.VoiceCommand voiceCommand = await this.voiceServiceConnection.GetVoiceCommandAsync();

                    CortanaBackgroundCommandBase backgroundCommand = null;
                    foreach (var command in this.commands)
                    {
                        if (command.CommandName.Equals(voiceCommand.CommandName, StringComparison.OrdinalIgnoreCase))
                        {
                            backgroundCommand = command;
                            break;
                        }
                    }

                    if (backgroundCommand != null)
                    {
                        await CortanaDialogHelper.ShowProgressScreen(this.voiceServiceConnection, StringResources.Message_LoadingTasks);

                        this.persistenceLayer = new WinPersistenceLayer(automaticSave: false);
                        var workbook = this.persistenceLayer.Open(tryUpgrade: true);
                        workbook.Initialize();

                        VoiceCommandResponse response = backgroundCommand.Execute(workbook, this.persistenceLayer, voiceCommand);

                        bool success = false;
                        if (response != null)
                        {
                            await this.voiceServiceConnection.ReportSuccessAsync(response);
                            success = true;
                        }

                        this.trackingManager.TagEvent("Cortana", new Dictionary<string, string>
                        {
                            { "Command", backgroundCommand.CommandName },
                            { "Success", success.ToString() },
                            { "Language", CultureInfo.CurrentUICulture?.ToString() ?? "unkown" }
                        });
                    }
                    else
                    {
                        // As with app activation VCDs, we need to handle the possibility that
                        // an app update may remove a voice command that is still registered.
                        // This can happen if the user hasn't run an app since an update.    
                        this.LaunchAppInForeground();
                    }
                }
                catch (Exception ex)
                {
                    if (this.trackingManager != null)
                        this.trackingManager.Exception(ex, "Cortana background service");
                }
            }
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
                this.serviceDeferral.Complete();
        }

        /// <summary>
        /// Provide a simple response that launches the app. Expected to be used in the
        /// case where the voice command could not be recognized (eg, a VCD/code mismatch.)
        /// </summary>
        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage
            {
                SpokenMessage = StringResources.Dialog_StartingApp
            };

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "";

            await this.voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        public async Task CancelAsync()
        {
            if (this.persistenceLayer != null)
                this.persistenceLayer.Save();

            await LogService.SaveAsync();
        }
    }
}
