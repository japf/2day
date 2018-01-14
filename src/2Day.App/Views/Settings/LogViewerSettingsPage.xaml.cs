using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Windows.ApplicationModel.Email;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class LogViewerSettingsPage : Page, ISettingsPage
    {
        public SettingsSizeMode Size { get { return SettingsSizeMode.Large; } }

        public LogViewerSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // called from "real" navigation
            this.OnNavigatedTo(null);
        }

        public async void OnNavigatedTo(object parameter)
        {
            // called from "flyout" navigation
            await this.LoadLogs();
        }

        private async Task LoadLogs()
        {
            await LogService.SaveAsync();
            var content = await LogService.LoadAsync();
            this.rtbLog.Blocks.Clear();

            var paragraph = new Paragraph();

            var lines = content.Split(new char[] {'\n'});
            if (lines.Length > 2000)
            {
                var newLines = new List<string>();
                newLines.Add("Showing only last 2000 entries, send logs by email to get full version");
                newLines.Add(string.Empty);

                int lowerBound = lines.Length - 2000;
                for (int i = lowerBound; i < lines.Length; i++)
                {
                    newLines.Add(lines[i]);
                }

                lines = newLines.ToArray();
            }

            foreach (var line in lines)
            {
                paragraph.Inlines.Add(new Run { Text = line });
            }
            this.rtbLog.Blocks.Add(paragraph);
        }

        private async void OnBtnClearLogs(object sender, RoutedEventArgs e)
        {
            await LogService.DeleteAsync();
            this.LoadLogs();
        }

        private void OnBtnGoEnd(object sender, RoutedEventArgs e)
        {
            this.scrollViewer.ChangeView(null, double.MaxValue, null);
        }

        private async void OnBtnMail(object sender, RoutedEventArgs e)
        {
            ITrackingManager trackingManager = Ioc.Resolve<ITrackingManager>();
            IMessageBoxService messageBoxService = Ioc.Resolve<IMessageBoxService>();

            try
            {
                var email = new EmailMessage
                {
                    Subject = "2Day Logs",
                    Body = "Logs are available as attachments"
                };

                await MailHelper.AddAttachment(email, "2day.db", "2day.db");
                await MailHelper.AddAttachment(email, LogService.Filename, "logs.txt");

                await EmailManager.ShowComposeNewEmailAsync(email);
            }
            catch (Exception ex)
            {
                string message = "Error while sending logs by email: " + ex;
                trackingManager.Exception(ex, message);
                messageBoxService.ShowAsync(StringResources.General_LabelError, message);
            }
        }
    }
}
