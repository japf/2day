using System;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public struct ExceptionInstance
    {
        public Exception Exception { get; private set; }
        public string Message { get; private set; }

        public ExceptionInstance(Exception exception, string message) : this()
        {
            this.Message = message;
            this.Exception = exception;
        }
    }
}