using System;
using System.Collections.Generic;
using Windows.Graphics.Display;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Notifications;

namespace Chartreuse.Today.Core.Universal.Manager
{
    /// <summary>
    /// Forward tracking events to a provider such as Application Insights. Disabled now that 2Day is open source.
    /// </summary>
    public class TrackingManager : ITrackingManager
    {
        public static Action<string, Dictionary<string, string>> TagEventHandler { get; set; }

        private readonly bool isEnabled;

        public TrackingManager(bool isEnabled, DeviceFamily deviceFamily = DeviceFamily.WindowsDesktop)
        {
            this.isEnabled = isEnabled;
#if DEBUG
            this.isEnabled = false;
#endif            
        }

        public void TagEvent(string name, Dictionary<string, string> attributes)
        {
            if (this.isEnabled)
            {
                // this.client.TrackEvent(name, attributes);

                if (TagEventHandler != null)
                {
                    try
                    {
                        TagEventHandler(name, attributes);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void Exception(Exception exception, string message = null, bool isFatal = false)
        {
            if (!this.isEnabled)
                return;

            if (TagEventHandler != null)
            {
                try
                {
                    TagEventHandler("Exception", new Dictionary<string, string>
                    {
                        { "fatal", isFatal.ToString() },
                        { "type", exception.GetType().Name }
                    });

                }
                catch (Exception)
                {
                }
            }

            string completeMessage = exception != null ? exception.ToString() : "no message";
            if (!string.IsNullOrWhiteSpace(message))
                completeMessage = string.Format("{0} ({1})", message, completeMessage);

            var parameters = GetTrackingParameters();
            parameters.Add("fatal", isFatal.ToString());
            parameters.Add("message", completeMessage ?? "n/a");

            // this.client.TrackException(exception, parameters);

            if (Ioc.HasType<IWorkbook>())
            {
                // toast message if debug is enabled
                IWorkbook workbook = Ioc.Resolve<IWorkbook>();
                if (workbook.Settings.GetValue<bool>(CoreSettings.DebugEnabled))
                    ToastHelper.ToastMessage("Exception", completeMessage);
            }

            LogService.Log("WinTrackingManager", string.Format("Unhandled exception: {0}", completeMessage));
        }

        public void Trace(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!this.isEnabled)
                return;

            var parameters = GetTrackingParameters();

            // this.client.TrackTrace(message, parameters);
        }

        public void Event(TrackingSource source, string category, string section)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            if (!this.isEnabled)
                return;

            LogService.Log(source.ToString(), string.Format("{0} {1}", category, section));

            var parameters = GetTrackingParameters();
            parameters.Add("category", category);
            parameters.Add("section", section);

            // this.client.TrackEvent(string.Format("{0} {1}", source, category), parameters);
        }
        
        private static Dictionary<string, string> GetTrackingParameters()
        {
            var dictionary = new Dictionary<string, string>();

            if (Ioc.HasType<ISynchronizationManager>() && Ioc.HasType<IWorkbook>())
            {
                var syncManager = Ioc.Resolve<ISynchronizationManager>();
                var workbook = Ioc.Resolve<IWorkbook>();

                dictionary.Add("sync", syncManager.ActiveService.ToString());
                if (syncManager.ActiveService == SynchronizationService.Exchange || syncManager.ActiveService == SynchronizationService.ExchangeEws)
                    dictionary.Add("server", workbook.Settings.GetValue<string>("ExchangeServerUri") ?? "n/a");
                if (syncManager.ActiveService == SynchronizationService.OutlookActiveSync || syncManager.ActiveService == SynchronizationService.ActiveSync)
                    dictionary.Add("server", workbook.Settings.GetValue<string>("ActiveSyncServerUri") ?? "n/a");

                dictionary.Add("lastSync", syncManager.Metadata != null ? syncManager.Metadata.LastSync.ToString("o") : "n/a");
                dictionary.Add("isSyncRunning", syncManager.IsSyncRunning.ToString());
            }

            if (Ioc.HasType<IStartupManager>())
            {
                var startupManager = Ioc.Resolve<IStartupManager>();
                dictionary.Add("uptime", startupManager.Uptime.ToString("g"));
            }

            try
            {
                dictionary.Add("orientation", DisplayInformation.GetForCurrentView().CurrentOrientation.ToString());
            }
            catch (Exception)
            {
                dictionary.Add("orientation", "unkown");
            }

            return dictionary;
            
        } 
    }
}
