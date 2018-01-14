using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Tools.Logging
{
    public static class LogService
    {
        public static string Filename { get; set; }

        private static readonly List<LogMessage> messages;

        private static ILogHandler logHandler;
        private static bool alwaysSave;
        private static LogLevel level;

        public static LogLevel Level
        {
            get { return level; }
            set
            {
                if (level != value)
                {
                    level = value;
                    Log("LogService", $"Log level set to {level}");
                }
            }
        }

        public static IEnumerable<LogLevel> AvailableLevels { get; private set; }

        public static IReadOnlyList<LogMessage> Messages
        {
            get { return new ReadOnlyCollection<LogMessage>(messages); }
        }

        static LogService()
        {
            AvailableLevels = new[] { LogLevel.None, LogLevel.Debug, LogLevel.Warning, LogLevel.Error, LogLevel.Network, LogLevel.Normal, LogLevel.Verbose };
            Filename = "activity.log";
            messages = new List<LogMessage>();
        }

        public static void Clear()
        {
            messages.Clear();
        }

        public static void Initialize(ILogHandler handler, bool saveAfterEachMessage = false)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            alwaysSave = saveAfterEachMessage;
            logHandler = handler;
        }

        public static async Task<string> LoadAsync()
        {
            if (logHandler != null)
                return await logHandler.LoadAsync(Filename);
            else
                return string.Empty;
        }

        public static async Task DeleteAsync()
        {
            if (logHandler != null)
                await logHandler.DeleteAsync(Filename);

            messages.Clear();
        }

        public static async Task SaveAsync()
        {
            if (logHandler != null)
            {
                var sb = new StringBuilder();
                foreach (var logMessage in messages)
                    sb.AppendLine(logMessage.ToString());

                await logHandler.SaveAsync(Filename, sb.ToString());
            }

            messages.Clear();
        }

        public static void Log(string source, string message)
        {
            Log(LogLevel.Debug, source, message);
        }

        public static void LogFormat(string source, string messageFormat, params object[] args)
        {
            Log(LogLevel.Debug, source, string.Format(messageFormat, args));
        }
        
#pragma warning disable 4014
        public static void Log(LogLevel level, string source, string message)
        {
            if (level == LogLevel.Normal)
                throw new ArgumentException(nameof(level));
            if (level == LogLevel.Verbose)
                throw new ArgumentException(nameof(level));

            Debug.WriteLine("{0}: {1}", source, message);

            if (Level == LogLevel.None || (level & Level) == 0)
                return;

            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            messages.Add(new LogMessage(level, source, message));

            if (alwaysSave)
                SaveAsync();
        }
#pragma warning restore 4014
    }
}
