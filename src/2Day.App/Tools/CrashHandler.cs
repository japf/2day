using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.Core.Universal.Tools;
using Chartreuse.Today.Exchange;

namespace Chartreuse.Today.App.Tools
{
    public class CrashHandler
    {
        private readonly Application app;

        public CrashHandler(Application app)
        {
            this.app = app;
            this.app.UnhandledException += this.OnUnhandledException;
        }

        public void TrackRootFrame(Frame rootFrame)
        {
            rootFrame.NavigationFailed += this.OnRootFrameNavigationFailed;
        }
        
        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            // nasty workaround: if we read e.Exception more than once, we loose the stack trace !
            Exception exception = unhandledExceptionEventArgs.Exception;

            unhandledExceptionEventArgs.Handled = true;

            // exit the app if we cannot recover the exception
            // e.Handled = !e.Exception.ToString().Contains(Constants.WinCollectionUIExceptionContent);

            string message = "n/a";
            if (!string.IsNullOrWhiteSpace(exception.Message))
                message = exception.Message;

            if (Ioc.HasType<ITrackingManager>())
                Ioc.Resolve<ITrackingManager>().Exception(exception, "OnUnhandledException: " + message, !unhandledExceptionEventArgs.Handled);

            LogService.Log("CrashHandler", "Exception: " + exception);
            await LogService.SaveAsync();

            bool isUnspecifiedError = message != null && message.ToLowerInvariant().Contains("unspecified error");
            if (unhandledExceptionEventArgs.Handled && !isUnspecifiedError)
            {
                this.SuggestExceptionEmail(exception, null);
            }
        }

        // Code to execute if a navigation fails
        private void OnRootFrameNavigationFailed(object sender, NavigationFailedEventArgs navigationFailedEventArgs)
        {
            // nasty workaround: if we read e.Exception more than once, we loose the stack trace !
            Exception exception = navigationFailedEventArgs.Exception;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
            else
            {
                string page = "unkown";
                if (navigationFailedEventArgs.SourcePageType != null)
                    page = navigationFailedEventArgs.SourcePageType.FullName;

                this.SuggestExceptionEmail(exception, page);
            }
        }

        private async void SuggestExceptionEmail(Exception e, string page)
        {
            var service = Ioc.Resolve<IMessageBoxService>();
            var platformService = Ioc.Resolve<IPlatformService>();

            TrackingManagerHelper.Exception(e, "Crash handler");

            var result = await service.ShowAsync(StringResources.General_LabelError, StringResources.Crash_MessageContent, DialogButton.YesNo);
            if (result == DialogResult.Yes)
            {
                var builder = new StringBuilder();

                builder.AppendLine("IMPORTANT: Please give details about the context of this error");
                builder.AppendLine();
                builder.AppendLine();

                builder.Append(GetDiagnosticsInformation(page));

                builder.AppendLine();
                builder.AppendLine();

                builder.AppendLine($"Exception: {e.Message}");
                builder.AppendLine($"Stacktrace: {ExceptionExtension.BeautifyAsyncStacktrace(e.StackTrace)}");
                
                if (e is AggregateException)
                {
                    var aggregateException = (AggregateException)e;
                    foreach (var innerException in aggregateException.InnerExceptions)
                    {
                        builder.AppendLine($"   Exception: {innerException.Message}");
                        builder.AppendLine($"   Stacktrace: {ExceptionExtension.BeautifyAsyncStacktrace(innerException.StackTrace)}");
                    }
                }

                await platformService.SendDiagnosticEmailAsync("2Day for Windows Error Report", builder.ToString());
            }            
        }

        public static string GetDiagnosticsInformation(string page = null)
        {
            IPlatformService platformService = Ioc.Resolve<IPlatformService>();

            DateTime now = DateTime.Now;

            if (page == null)
            {
                try
                {
                    var frame = Window.Current.Content as Frame;
                    if (frame != null && frame.Content != null)
                        page = frame.Content.GetType().Name;
                }
                catch (Exception)
                {
                }
            }

            string installDate = string.Empty;
            string backgroundLastStatus = string.Empty;
            string backgroundLastStart = string.Empty;
            string backgroundLastEnd = string.Empty;
            string backgroundAccess = string.Empty;
            string backgroundSync = string.Empty;
            try
            {
                installDate = WinSettings.Instance.GetValue<DateTime>(CoreSettings.InstallDateTime).ToString("G");
                backgroundLastStatus = WinSettings.Instance.GetValue<BackgroundExecutionStatus>(CoreSettings.BackgroundLastStatus).ToString();
                backgroundLastStart = WinSettings.Instance.GetValue<DateTime>(CoreSettings.BackgroundLastStartExecution).ToString("G");
                backgroundLastEnd = WinSettings.Instance.GetValue<DateTime>(CoreSettings.BackgroundLastEndExecution).ToString("G");
                backgroundAccess = WinSettings.Instance.GetValue<bool>(CoreSettings.BackgroundAccess).ToString();
                backgroundSync = WinSettings.Instance.GetValue<bool>(CoreSettings.BackgroundSync).ToString();
            }
            catch (Exception)
            {
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Version: {ApplicationVersion.GetAppVersion()}");
            builder.AppendLine($"Windows: {platformService.WindowsVersion}");
            builder.AppendLine($"Platform: {GetDeviceInfo()}");
            builder.AppendLine($"Resolution: {GetScreenResolution()}");
            builder.AppendLine($"Orientation: {GetScreenOrientation()}");
            builder.AppendLine($"Page: {page}");
            builder.AppendLine($"Crash date: {now.ToString("G")}");
            builder.AppendLine($"Install date: {installDate}");
            builder.AppendLine($"Background enabled: {backgroundAccess}");
            builder.AppendLine($"Background sync: {backgroundSync}");
            builder.AppendLine($"Background status: {backgroundLastStatus}");
            builder.AppendLine($"Background start: {backgroundLastStart}");
            builder.AppendLine($"Background end: {backgroundLastEnd}");

            if (Ioc.HasType<IStartupManager>())
            {
                var startupManager = Ioc.Resolve<IStartupManager>();
                builder.AppendLine($"Uptime: {startupManager.Uptime}");
            }

            builder.AppendLine($"Culture: {CultureInfo.CurrentUICulture}");
            builder.AppendLine($"Timezone: {TimeZoneInfo.Local}" );

            builder.AppendLine($"DeviceId: {platformService.DeviceId}");
            builder.AppendLine($"DeviceFamily: {platformService.DeviceFamily}");

            try
            {
                if (Ioc.HasType<ISynchronizationManager>() && Ioc.HasType<IWorkbook>())
                {
                    var synchronizationManager = Ioc.Resolve<ISynchronizationManager>();
                    var workbook = Ioc.Resolve<IWorkbook>();

                    builder.AppendLine(string.Format("Ordering1: {0} {1}", workbook.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType1), workbook.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending1)));
                    builder.AppendLine(string.Format("Ordering2: {0} {1}", workbook.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType2), workbook.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending2)));
                    builder.AppendLine(string.Format("Ordering3: {0} {1}", workbook.Settings.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType3), workbook.Settings.GetValue<bool>(CoreSettings.TaskOrderingAscending3)));
                    builder.AppendLine();
                    builder.AppendLine(string.Format("Sync provider: {0}", synchronizationManager.ActiveService));
                    builder.AppendLine(string.Format("Last sync: {0}", synchronizationManager.Metadata != null ? synchronizationManager.Metadata.LastSync.ToString("G") : "N/A"));
                    builder.AppendLine(string.Format("Sync in progress {0}", synchronizationManager.IsSyncRunning));
                    builder.AppendLine(string.Format("Sync at startup: {0}", workbook.Settings.GetValue<bool>(CoreSettings.SyncOnStartup).ToString()));
                    builder.AppendLine(string.Format("Sync auto: {0}", workbook.Settings.GetValue<bool>(CoreSettings.SyncAutomatic).ToString()));
                    builder.AppendLine(string.Format("Sync background: {0}", workbook.Settings.GetValue<bool>(CoreSettings.BackgroundSync).ToString()));
                    builder.AppendLine(string.Format("Sync user id: {0}", workbook.Settings.GetValue<string>(CoreSettings.SyncUserId)));

                    if (workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri) != null)
                        builder.AppendLine(string.Format("Active Sync Server uri: {0}", workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri)));
                    if (workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri) != null)
                        builder.AppendLine(string.Format("Exchange Server uri: {0}", workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri)));
                }
                else
                {
                    builder.Append("No synchronization manager available");
                }
            }
            catch (Exception e)
            {
                builder.AppendLine($"Error while getting additional info ({e})");
            }

            builder.AppendLine();

            return builder.ToString();
        }
        
        private static string GetDeviceInfo()
        {
            try
            {
                EasClientDeviceInformation deviceInformation = new EasClientDeviceInformation();
                string productName = !string.IsNullOrEmpty(deviceInformation.SystemProductName) ? deviceInformation.SystemProductName : deviceInformation.SystemSku;
                return deviceInformation.SystemManufacturer + " " + productName;
            }
            catch (Exception)
            {
                return "Error while getting device info";
            }
        }

        private static string GetScreenResolution()
        {
            try
            {
                if (Window.Current == null)
                    return "Unknown";

                Rect bounds = Window.Current.Bounds;

                return string.Format("{0}x{1}", bounds.Width, bounds.Height);
            }
            catch (Exception)
            {
                return "Unkown";
            }
        }

        private static string GetScreenOrientation()
        {
            try
            {
                return DisplayInformation.GetForCurrentView().CurrentOrientation.ToString();
            }
            catch (Exception)
            {
                return "Unkown";
            }
        }
    }
}
