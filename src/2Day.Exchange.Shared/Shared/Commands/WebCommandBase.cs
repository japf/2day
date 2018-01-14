using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Exchange.ActiveSync.Exceptions;

namespace Chartreuse.Today.Exchange.Shared.Commands
{
    public abstract class WebCommandBase<TRequestBuilder, TResponseParser, TSettings> : IWebCommandBase<TResponseParser>
        where TRequestBuilder : IRequestParameterBuilder
        where TResponseParser : IResponseParser, new()
    {
        private readonly CommandParameterBase<TRequestBuilder, TSettings> parameters;

        public abstract string CommandName { get; }

        protected TRequestBuilder Parameters
        {
            get { return this.parameters.Parameter; }
        }

        protected TSettings Settings
        {
            get { return this.parameters.Settings; }
        }

        protected WebCommandBase(CommandParameterBase<TRequestBuilder, TSettings> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            this.parameters = parameters;
        }

        public abstract Task<ResponseResult<TResponseParser>> Execute();

        protected ResponseResult<TResponseParser> CreateResponseResult(Exception exception, WebRequestResponse response, string uri)
        {
            string exceptionMessage = null;
            string statusCodeMessage = null;

            if (exception != null)
            {
                exceptionMessage += $"Exception: {exception.Message}";
                if (!string.IsNullOrWhiteSpace(exception.InnerException?.Message))
                    exceptionMessage += $" ({exception.InnerException.Message}).";
                else
                    exceptionMessage += ".";
            }

            if (response != null)
            {
                if (response.Response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.Response.StatusCode == HttpStatusCode.Unauthorized)
                        statusCodeMessage = "Got 'Unauthorized' response status code, check credentials (email, login, password) are correct.";
                    else
                        statusCodeMessage = $"Got '{response.Response.StatusCode}' response status code.";
                }
            }

            HttpResponseMessage responseMessage = null;
            if (response != null && response.Response != null)
                responseMessage = response.Response;

            string message = exceptionMessage;
            if (statusCodeMessage != null)
            {
                if (message != null)
                    message += exceptionMessage;
                else
                    message = statusCodeMessage;
            }

            return ResponseResult<TResponseParser>.Create(
                new CommandException($"Error while executing '{this.CommandName}'", message, exception, responseMessage),
                uri);
        }
    }
}
