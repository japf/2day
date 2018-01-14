using System;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Exchange.Ews.Exceptions
{
    /// <summary>
    /// Occurs when the XML received in a command response is invalid
    /// </summary>
    public class CommandResponseXmlException : Exception
    {
        public CommandResponseXmlException(string message)
            : base(message)
        {
            LogService.Log("CommandResponseXmlException", message);
        }
    }
}