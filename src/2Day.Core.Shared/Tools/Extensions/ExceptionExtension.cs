using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    /// <summary>
    /// An helper class to make exception prettier :-)
    /// </summary>
    public static class ExceptionExtension
    {
        private static readonly List<string> AsyncRemovals = new List<string>
        {
            "--- End of stack trace from previous location where exception was thrown ---",
            "at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)",
            "at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)",
            "at System.Runtime.CompilerServices.TaskAwaiter.ValidateEnd(Task task)",
            "at System.Runtime.CompilerServices.TaskAwaiter.GetResult()",
            "at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()",
            "at System.Runtime.CompilerServices.TaskAwaiter`2.GetResult()",
            "at System.Runtime.CompilerServices.TaskAwaiter`3.GetResult()",
            "at System.Runtime.CompilerServices.TaskAwaiter`4.GetResult()",
            "Chartreuse.Today.",
            ".MoveNext()"
        }; 

        /// <summary>
        /// Remove async generated code from a stacktrace
        /// </summary>
        /// <param name="stacktrace">Source stacktrace with async generate code</param>
        /// <returns>Cleaned up stacktrace</returns>
        public static string BeautifyAsyncStacktrace(string stacktrace)
        {
            if (string.IsNullOrEmpty(stacktrace))
                return stacktrace;

            foreach (var removal in AsyncRemovals)
            {
                stacktrace = stacktrace.Replace("   " + removal + "\r\n", string.Empty);
                stacktrace = stacktrace.Replace(removal + "\r\n", string.Empty);
                stacktrace = stacktrace.Replace(removal, string.Empty);
            }

            stacktrace.Replace("\r\n\r\n", "\r\n");
            stacktrace.Replace("\n\n", "\n");

            return stacktrace;
        }

        /// <summary>
        /// Recursively get all message from an exception by walking in the InnerException property
        /// </summary>
        /// <param name="exp">Exception</param>
        /// <returns>A string containing all the exception messages</returns>
        public static string GetAllMessages(this Exception exp)
        {
            string message = string.Empty;
            Exception innerException = exp;

            do
            {
                message = message + (string.IsNullOrEmpty(innerException.Message) ? string.Empty : innerException.Message);
                innerException = innerException.InnerException;
            }
            while (innerException != null);

            return message;
        }
    }
}
