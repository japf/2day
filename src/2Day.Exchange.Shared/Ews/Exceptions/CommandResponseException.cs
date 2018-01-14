using System;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Exchange.Ews.Exceptions
{
    /// <summary>
    /// Occurs when the response leads to an invalid state
    /// </summary>
    public class CommandResponseException : Exception
    {
        public CommandResponseException(string message)
            : base(message)
        {
            LogService.Log("CommandResponseException", message);
        }  

        public CommandResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
            LogService.Log("CommandResponseException", message);
        }        
    }
}