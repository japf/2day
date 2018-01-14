using System;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Exchange.Ews.Exceptions
{
    /// <summary>
    /// Occurs when a command is attempted to be executed while it's parameter are invalid
    /// </summary>
    public class CommandCannotExecuteException : Exception
    {
        public CommandCannotExecuteException(string message) : base(message)
        {
            LogService.Log("CommandCannotExecuteException", message);
        }
    }
}
