using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Shared;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public abstract class EwsCommandBase<TRequestBuilder, TResponseParser> : WebCommandBase<TRequestBuilder, TResponseParser, EwsRequestSettings>
        where TRequestBuilder : IRequestParameterBuilder
        where TResponseParser : IResponseParser, new()
    {
        private const string ContentTypeTextXml = "text/xml";

        private WebRequestBuilder webRequestBuilder;

        protected EwsCommandBase(EwsRequestParameter<TRequestBuilder> parameters)
            : base(parameters)
        {
        }

        protected EwsCommandBase(EwsRequestParameter<TRequestBuilder> parameters, WebRequestBuilder webRequestBuilder)
            : base(parameters)
        {
            this.webRequestBuilder = webRequestBuilder;
        }

        public override async Task<ResponseResult<TResponseParser>> Execute()
        {
            this.CheckSettings();

            if (this.webRequestBuilder == null)
                this.webRequestBuilder = new EwsWebRequestBuilder();

            string xml = this.Parameters.BuildXml(this.CommandName);

            return await this.SendRequestAsync(xml, this.Settings.Username, this.Settings.Password);
        }

        private async Task<ResponseResult<TResponseParser>> SendRequestAsync(string xml, string username, string password, bool secondTry = false)
        {
            WebRequestResponse response = null;
            try
            {
                response = await this.webRequestBuilder.SendRequestAsync(
                    this.Settings.ServerUri,
                    HttpMethod.Post,
                    ContentTypeTextXml,
                    xml,
                    new Dictionary<string, string>(),
                    new NetworkCredential(username, password));

                if (response.Response.IsSuccessStatusCode)
                {
                    var parser = new TResponseParser();
                    parser.ParseResponse(this.CommandName, response);

                    return ResponseResult<TResponseParser>.Create(parser, this.Settings.ServerUri);
                }
                else if (!secondTry && response.Response.StatusCode == HttpStatusCode.Unauthorized && this.Settings.Email != null && this.Settings.Username != this.Settings.Email)                
                {
                    // one more try using email for authentication
                    return await this.SendRequestAsync(xml, this.Settings.Email, this.Settings.Password, true);
                }
                else
                {
                    return this.CreateResponseResult(null, response, this.Settings.ServerUri);
                }
            }
            catch (Exception ex)
            {
                return this.CreateResponseResult(ex, response, this.Settings.ServerUri);
            }
        }

        private void CheckSettings()
        {
            if (string.IsNullOrWhiteSpace(this.Settings.ServerUri))
                throw new CommandCannotExecuteException("Server uri is null, empty or whitespace");

            if (string.IsNullOrWhiteSpace(this.Settings.Email))
                throw new CommandCannotExecuteException("Email is null, empty or whitespace");

            if (string.IsNullOrWhiteSpace(this.Settings.Password))
                throw new CommandCannotExecuteException("Password is null, empty or whitespace");
        }
    }
}
