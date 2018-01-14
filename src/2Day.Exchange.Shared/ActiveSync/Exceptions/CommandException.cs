using System;
using System.Net.Http;

namespace Chartreuse.Today.Exchange.ActiveSync.Exceptions
{
    public class CommandException : Exception
    {
        private readonly string innerMessage;
        private readonly HttpResponseMessage response;

        public string InnerMessage
        {
            get { return this.innerMessage; }
        }

        public HttpResponseMessage Response
        {
            get { return this.response; }
        }


        internal CommandException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        internal CommandException(string message, string innerMessage, Exception innerException)
            :base(message, innerException)
        {
            this.innerMessage = innerMessage;
        }

        internal CommandException(string message, string innerMessage, Exception innerException, HttpResponseMessage response)
            : base(message, innerException)
        {
            this.innerMessage = innerMessage;
            this.response = response;
        }
    }
}
