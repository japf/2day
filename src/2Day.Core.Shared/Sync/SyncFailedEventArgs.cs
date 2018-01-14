using System;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public class SyncFailedEventArgs : EventArgs
    {
        public SyncFailedEventArgs(string message, Exception exception)
        {
            this.Exception = exception;
            this.Message = message;
        }

        public string Message { get; private set; }
        public Exception Exception { get; private set; }
    }
}
