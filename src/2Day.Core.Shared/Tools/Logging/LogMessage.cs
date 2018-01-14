using System;

namespace Chartreuse.Today.Core.Shared.Tools.Logging
{
    public class LogMessage
    {
        public DateTime DateTime { get; private set; }
        public LogLevel Level { get; private set; }
        public string Source { get; private set; }
        public string Message { get; private set; }

        public LogMessage(LogLevel level, string source, string message)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            this.DateTime = DateTime.Now;
            this.Level = level;
            this.Source = source;
            this.Message = message;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1} {2}: {3}", this.DateTime, this.DateTime.Millisecond, this.Source, this.Message);
        }
    }
}
