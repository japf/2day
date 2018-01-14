using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Windows.Storage;

namespace Chartreuse.Today.Core.Universal.Model
{
    public class WinSettings : ISettings
    {
        private readonly Dictionary<string, object> defaultValues;

        public static WinSettings Instance = new WinSettings();

        public event EventHandler<SettingsKeyChanged> KeyChanged;

        public WinSettings()
        {
            this.defaultValues = new Dictionary<string, object>
                                     {
                                         { CoreSettings.BackgroundSync               , false },
                                         { CoreSettings.SyncAutomatic                , false },
                                         
                                         { CoreSettings.AutoDeleteFrequency          , AutoDeleteFrequency.OneWeek },
                                         { CoreSettings.UseGroupedDates              , false },
                                         { CoreSettings.IncludeNoDateInViews         , false },
                                         { CoreSettings.LaunchCountBeforeReview      , 15 },
                                         { CoreSettings.FirstDayOfWeek               , DayOfWeek.Monday },
                                         { CoreSettings.CompletedTasksMode           , CompletedTaskMode.Hide },
                                         { CoreSettings.AutoDeleteTags               , true },
                                         
                                         { CoreSettings.TaskOrderingType1            , TaskOrdering.Priority },
                                         { CoreSettings.TaskOrderingAscending1       , false },
                                         { CoreSettings.TaskOrderingType2            , TaskOrdering.Folder },
                                         { CoreSettings.TaskOrderingAscending2       , true },
                                         { CoreSettings.TaskOrderingType3            , TaskOrdering.Alphabetical },
                                         { CoreSettings.TaskOrderingAscending3       , true },

                                         { CoreSettings.UseDarkTheme                 , true },
                                         { CoreSettings.SendAnalytics                , true },
                                         { CoreSettings.BadgeValue                   , "view-1" },

                                         { CoreSettings.BackgroundPattern         , "office"},
                                         { CoreSettings.BackgroundOpacity         , 1.0},
                                         { CoreSettings.NavigationMenuMinimized   , false }
                                     };
        }

        public bool HasValue(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        }

        public T GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                string item = (string)ApplicationData.Current.LocalSettings.Values[key];
                return Deserialize<T>(item);
            }

            if (this.defaultValues.ContainsKey(key))
            {
                return (T)this.defaultValues[key];
            }
            else
            {
                return default(T);
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values[key] = Serialize<T>(value);
                              
                if (!this.defaultValues.ContainsKey(key) || !this.defaultValues[key].Equals(value))
                    this.KeyChanged.Raise(this, new SettingsKeyChanged(key, value));
            }
            else
            {
                string item = ApplicationData.Current.LocalSettings.Values[key] as string;
                bool update = false;

                if (item == null)
                {
                    update = true;
                }
                else
                {
                    try
                    {
                        var deserializedValue = Deserialize<T>(item);
                        if ((deserializedValue == null && value != null) || (deserializedValue != null && !deserializedValue.Equals(value)))
                        {
                            update = true;
                        }
                    }
                    catch (Exception)
                    {
                        if (value == null)
                        {
                            ApplicationData.Current.LocalSettings.Values[key] = null;
                            this.KeyChanged.Raise(this, new SettingsKeyChanged(key, value));
                        }

                        return;
                    }
                }

                if (update)
                {
                    ApplicationData.Current.LocalSettings.Values[key] = Serialize<T>(value);
                    this.KeyChanged.Raise(this, new SettingsKeyChanged(key, value));
                }
            }
        }

        private static string Serialize<T>(T value)
        {
            var serializer = new XmlSerializer(typeof (T));
            var stream = new StringWriter();
            serializer.Serialize(stream, value);

            return stream.ToString();
        }

        public static T Deserialize<T>(string value)
        {
            var serializer = new XmlSerializer(typeof(T));
            var stream = new StringReader(value);

            return (T)serializer.Deserialize(stream);
        }
    }
}
