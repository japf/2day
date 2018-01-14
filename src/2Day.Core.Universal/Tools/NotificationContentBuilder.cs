using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Converter;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
#if NETFX_CORE
using Windows.Data.Xml.Dom;
#else
using System.Xml; // for use with unit test
#endif

namespace Chartreuse.Today.Core.Universal.Tools
{
    /// <summary>
    /// This creates XmlDocument objets that can use to update a live tile or to send a toast notification
    /// </summary>
    public static class NotificationContentBuilder
    {
        public static XmlDocument CreateSimpleToastNotification(string title, string message, string launchArgument = null)
        {
            string content = string.Empty;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                const string templateXml = @"
                    <toast{0}>
                        <visual>
                            <binding template='ToastGeneric'>
                                <text>{1}</text>
                                <text>{2}</text>
                            </binding>
                        </visual>
                    </toast>
                    ";

                content = string.Format(
                    templateXml,
                    launchArgument != null ? string.Format(" launch='{0}'", launchArgument) : string.Empty, // 0
                    SafeEscape(title), // 1
                    SafeEscape(message) // 2
                    );

                xmlDocument.LoadXml(content);

                return xmlDocument;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("Exception CreateSimpleToastNotification: {0}", content));

                return null;
            }
        }

        public static XmlDocument CreateTaskToastNotification(ITask task)
        {
            string content = string.Empty;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                const string templateXml = @"
                    <toast scenario='reminder'>
                        <visual>
                            <binding template='ToastGeneric'>
                                <text>{0}</text>
                                <text>{1}</text>
                                <image placement='AppLogoOverride' src='{2}' />
                            </binding>
                        </visual>
                        <actions>
                            <input id='snoozeTime' type='selection' defaultInput='5'>
                              <selection id='5' content='5 {3}' />
                              <selection id='30' content='30 {3}' />
                              <selection id='60' content='1 {4}' />
                              <selection id='240' content='4 {5}' />
                              <selection id='1440' content='1 {6}' />
                            </input>
                            <action activationType='system' arguments='snooze' hint-inputId='snoozeTime' content=''/>
                            <action content='{7}' arguments='{8}' activationType='background' />
                            <action content='{9}' arguments='{10}' activationType='foreground' />
                        </actions>
                        <audio src='ms-winsoundevent:Notification.Reminder' />
                    </toast>
                    ";

                content = string.Format(
                    templateXml,
                    SafeEscape(task.Title),                         // 0
                    SafeEscape(task.Note ?? string.Empty),          // 1
                    ResourcesLocator.GetAppIconPng(),               // 2
                    StringResources.Notification_SnoozeMinutes,     // 3
                    StringResources.Notification_SnoozeHour,        // 4
                    StringResources.Notification_SnoozeHours,       // 5
                    StringResources.Notification_SnoozeDay,         // 6
                    StringResources.Notification_Done,              // 7
                    LaunchArgumentsHelper.GetArgCompleteTask(task), // 8
                    StringResources.Notification_Edit,              // 9
                    LaunchArgumentsHelper.GetArgEditTask(task)      // 10
                    );

                xmlDocument.LoadXml(content);

                return xmlDocument;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("Exception CreateTaskToastNotification: {0}", content));
                return null;
            }
        }

        public static XmlDocument CreateTileNotificationForTask(ITask task)
        {
            string content = string.Empty;
            try
            {
                var xmlDocument = new XmlDocument();

                const string templateXml = @"
                    <tile>
                      <visual branding=""nameAndLogo"">

                        <binding template=""TileSmall"" hint-textStacking=""center"">
                          <text hint-style=""subtitle"" hint-align=""center"">{0}</text>
                        </binding>

                        <binding template=""TileMedium"" displayName=""{3}"">
                          <text hint-style=""base"">{0}</text>
                          <text hint-style=""caption"">{1}</text>
                          <text hint-style=""captionSubtle"" hint-wrap=""true"">{2}</text>
                        </binding>

                        <binding template=""TileWide"" displayName=""{4}"">
                          <group>
                            <subgroup hint-weight=""33"">
                              <image src=""{5}"" />
                            </subgroup>
                            <subgroup>
                              <text hint-style=""base"">{0}</text>
                              <text hint-style=""caption"">{1}</text>
                              <text hint-style=""captionSubtle"" hint-wrap=""true"">{2}</text>
                            </subgroup>
                          </group>
                        </binding>

                        <binding template=""TileLarge"" displayName=""{4}"">
                          <group>
                            <subgroup>
                              <text hint-style=""base"">{0}</text>
                             <text hint-style=""caption"">{1}</text>
                              <text hint-style=""captionSubtle"" hint-wrap=""true"">{2}</text>
                              <image hint-align=""center"" src=""{5}"" />
                            </subgroup>
                          </group>
                        </binding>

                      </visual>
                    </tile>
                    ";

                string picture = ResourcesLocator.GetFolderIconPng(task.Folder);

                content = string.Format(
                    templateXml,
                    SafeEscape(task.Title),                                             // 0 title
                    SafeEscape(task.Folder.Name),                                       // 1 folder
                    SafeEscape(task.Note ?? string.Empty),                              // 2 notes (max: 65 characters)
                    task.Due.HasValue ? task.Due.Value.ToString("M") : string.Empty,    // 3 short due (Aug. 22)
                    task.Due.HasValue ? RelativeDateConverter.ConvertRelative(task.Due) : string.Empty,    // 4 long due (Today Aug. 22)
                    picture                                                             // 5 image
                );

                xmlDocument.LoadXml(content);

                return xmlDocument;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("Exception CreateTileNotification: {0}", content));
                return null;
            }
        }

        public static XmlDocument CreateTileNotification(string title, IList<ITask> tasks, IAbstractFolder folder = null)
        {
            string content = string.Empty;
            try
            {
                var xmlDocument = new XmlDocument();

                const string templateXml = @"
                    <tile>
                        <visual branding=""nameAndLogo"" displayName=""{0}"">
                            <binding template=""TileSmall"" hint-textStacking=""center"" branding=""none"">
                                <image placement=""peek"" src=""{3}""/>
                                <text hint-align=""center"" hint-style=""title"">{1}</text>
                            </binding>
                            <binding template=""TileMedium"">
                                {2}
                            </binding>
                            <binding template=""TileWide"" hint-lockDetailedStatus1=""{4}"" hint-lockDetailedStatus2=""{5}"" hint-lockDetailedStatus3=""{6}"">
                                <group>
                                    <subgroup hint-weight=""20"" hint-textStacking=""center"">
                                        <image src=""{3}""/>
                                    </subgroup>
                                    <subgroup hint-weight=""80"" hint-textStacking=""center"">
                                        {2}
                                    </subgroup>
                                </group>
                            </binding>
                            <binding template=""TileLarge"">
                                {2}
                            </binding>
                        </visual>
                    </tile>
                    ";

                var builder = new StringBuilder();
                bool hasTask = false;

                string task1 = string.Empty;
                string task2 = string.Empty;
                string task3 = string.Empty;

                for (int i = 0; i < 10; i++)
                {
                    string taskName = TryGetTaskAt(tasks, i);
                    if (!string.IsNullOrWhiteSpace(taskName))
                    {
                        var safeEscapeTaskName = SafeEscape(taskName);
                        builder.AppendLine(string.Format("<text>{0}</text>", safeEscapeTaskName));
                        hasTask = true;

                        if (i == 0)
                            task1 = safeEscapeTaskName;
                        else if (i == 1)
                            task2 = safeEscapeTaskName;
                        else if (i == 2)
                            task3 = safeEscapeTaskName;
                    }
                }

                if (!hasTask)
                {
                    if (folder != null)
                        builder.AppendLine(string.Format("<text hint-style='captionsubtle' hint-wrap='true'>{0}</text>", SafeEscape(folder.EmptyHeader)));
                    else
                        builder.AppendLine(string.Format("<text hint-style='captionsubtle' hint-wrap='true'>{0}</text>", SafeEscape(StringResources.SystemView_Today_EmptyHeader)));
                }

                string displayName = SafeEscape(title);
                string counter = tasks.Count.ToString(CultureInfo.InvariantCulture);
                string text = builder.ToString();
                string picture = folder != null ? ResourcesLocator.GetFolderIconPng(folder) : ResourcesLocator.GetAppIconPng();

                content = string.Format(
                    templateXml,
                    displayName,   // 0
                    counter,       // 1
                    text,          // 2
                    picture,       // 3
                    task1,         // 4
                    task2,         // 5
                    task3          // 6
                );

                xmlDocument.LoadXml(content);

                return xmlDocument;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("Exception CreateTileNotification: {0}", content));
                return null;
            }
        }

        private static string SafeEscape(string content, int? maxLength = null)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            if (content.Length > 100)
            {
                content = string.Format("{0}...", content.Substring(0, 100));
            }

            content = content.StripHtml();
            content = content.ToEscapedXml();
            content = content.Replace(@"""", "&quot;");

            if (maxLength.HasValue && content.Length > maxLength.Value)
                content = content.Substring(0, maxLength.Value);

            return content;
        }

        private static string TryGetTaskAt(IList<ITask> tasks, int index)
        {
            if (index < tasks.Count)
            {
                ITask task = tasks[index];

                if (task.IsLate)
                    return string.Format("{0} {1}", "!", task.Title);
                else
                return task.Title;
            }
            else
            {
                return string.Empty;
            }
        }       
    }
}
