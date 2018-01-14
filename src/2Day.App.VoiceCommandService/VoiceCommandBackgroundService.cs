using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Chartreuse.Today.App.VoiceCommand;

namespace Chartreuse.Today.App.VoiceCommandService
{
    public sealed class VoiceCommandBackgroundService : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private CortanaBackgroundService cortanaBackgroundService;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            this.deferral = taskInstance.GetDeferral();

            // Register to receive an event if Cortana dismisses the background task. This will
            // occur if the task takes too long to respond, or if Cortana's UI is dismissed.
            // Any pending operations should be cancelled or waited on to clean up where possible.
            taskInstance.Canceled += this.OnTaskCanceled;

            this.cortanaBackgroundService = new CortanaBackgroundService(taskInstance.TriggerDetails as AppServiceTriggerDetails, this.deferral);

            await this.cortanaBackgroundService.HandleCommandAsync();

            this.deferral.Complete();
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.cortanaBackgroundService != null)
                this.cortanaBackgroundService.CancelAsync();

            if (this.deferral != null)
                this.deferral.Complete();
        }
    }
}
