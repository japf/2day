using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Task = System.Threading.Tasks.Task;

namespace Chartreuse.Today.Core.Shared.Manager
{
    public class StartupManager : IStartupManager
    {
        private readonly IWorkbook workbook;
        private readonly IPlatformService platformService;
        private readonly ISettings settings;
        private readonly IMessageBoxService messageBoxService;
        private readonly INotificationService notificationService;
        private readonly ITrackingManager trackingManager;
        private readonly DateTime startTime;

        public TimeSpan Uptime
        {
            get { return DateTime.UtcNow - this.startTime; }
        }

        public bool IsFirstLaunch
        {
            get { return this.settings.GetValue<int>(CoreSettings.LaunchCount) == 0; }
        }
        
        public StartupManager(IWorkbook workbook, IPlatformService platformService, IMessageBoxService messageBoxService, INotificationService notificationService, ITrackingManager trackingManager)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (notificationService == null)
                throw new ArgumentNullException(nameof(notificationService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.workbook = workbook;
            this.platformService = platformService;
            this.settings = workbook.Settings;
            this.messageBoxService = messageBoxService;
            this.notificationService = notificationService;
            this.trackingManager = trackingManager;

            this.startTime = DateTime.UtcNow;
        }

        public async Task<bool> HandleStartupAsync()
        {
            this.settings.SetValue(CoreSettings.LastRunDateTime, DateTime.UtcNow);

            int launchCount = this.settings.GetValue<int>(CoreSettings.LaunchCount);

            if (launchCount > 0)
            {                
                string lastVersion = this.settings.GetValue<string>(CoreSettings.LastVersion);
                if (!string.IsNullOrWhiteSpace(lastVersion) && lastVersion != this.platformService.Version)
                {
                    // new version installed
                    this.settings.SetValue(CoreSettings.LastVersion, this.platformService.Version);
                    this.HandleNewVersionInstalled();
                }

                if (launchCount == this.settings.GetValue<int>(CoreSettings.LaunchCountBeforeReview))
                {
                    // the app has been launched XX numer of times while not in trial,
                    // show a message and ask the user to review the app
                    await this.HandleReviewProposition();
                }
            }
            
            launchCount++;
            this.settings.SetValue(CoreSettings.LaunchCount, launchCount);

            return true;
        }
        
        private async Task OpenReviewPaneAsync()
        {
            await this.platformService.OpenWebUri(Constants.MarketplaceReviewAppUri);
        }

        private async Task HandleReviewProposition()
        {
            string[] buttons = null;

            if (this.platformService.DeviceFamily == DeviceFamily.WindowsMobile)
            {
                buttons = new[]
                {
                    StringResources.MessageReview_Yes,
                    StringResources.MessageReview_No,
                };
            }
            else
            {
                buttons = new[]
                {
                    StringResources.MessageReview_Yes,
                    StringResources.MessageReview_Later,
                    StringResources.MessageReview_No,
                    //StringResources.MessageReview_CreateTask, 
                    //StringResources.MessageReview_AlreadyDone, 
                };
            }

            var result = await this.messageBoxService.ShowAsync(
                StringResources.Message_Information,
                StringResources.MessageReview_Body,
                buttons);

            int index = result.ButtonIndex;

            if (index == 1 && this.platformService.DeviceFamily == DeviceFamily.WindowsMobile)
                index = 2;

            string mode = "none";

            switch (index)
            {
                case 0:
                    mode = "now";
                    await this.OpenReviewPaneAsync();

                    this.settings.SetValue(CoreSettings.LaunchCountBeforeReview, -1);

                    break;

                case 1:
                    mode = "later";
                    int nextLaunchCount = this.settings.GetValue<int>(CoreSettings.LaunchCountBeforeReview) + 10;
                    this.settings.SetValue(CoreSettings.LaunchCountBeforeReview, nextLaunchCount);
                    break;

                case 2:
                    mode = "no";
                    this.settings.SetValue(CoreSettings.LaunchCountBeforeReview, -1);
                    break;

                case 3:
                    mode = "task";
                    if (this.workbook.Folders.Any())
                    {
                        var now = DateTime.Now;
                        var reviewTask = new Model.Impl.Task
                        {
                            Title = StringResources.MessageReview_TaskTitle,
                            Note = StringResources.MessageReview_TaskNote,
                            Due = now,
                            Added = now,
                            Modified = now,
                            Folder = this.workbook.Folders.FirstOrDefault()
                        };
                    }

                    this.settings.SetValue(CoreSettings.LaunchCountBeforeReview, -1);
                    break;

                case 4:
                    mode = "already-done";
                    this.settings.SetValue(CoreSettings.LaunchCountBeforeReview, -1);
                    break;
            }

            this.trackingManager.TagEvent("Store review", new Dictionary<string, string> { { "mode", mode } });
        }

        public async Task HandleFirstRunAsync()
        {
        }

        private void HandleNewVersionInstalled()
        {
            this.notificationService.ShowNativeToast(
                StringResources.Message_AppUpdatedTitle,
                StringResources.Message_AppUpdatedMessage, 
                Constants.WindowsChangelogAddress);
        }
    }
}

