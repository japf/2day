using System;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Exchange.Ews.Exceptions
{
    /// <summary>
    /// Occurs when a command fails because of authentication
    /// </summary>
    public class CommandAuthorizationException : Exception
    {
        public CommandAuthorizationException(string message)
            : base(message)
        {
            LogService.Log("CommandAuthorizationException", message);
        }
    }
}