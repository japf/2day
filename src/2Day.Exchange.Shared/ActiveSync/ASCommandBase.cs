using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Shared;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    internal abstract class ASCommandBase<TRequestBuilder, TResponseParser> : WebCommandBase<TRequestBuilder, TResponseParser, ASRequestSettings>
        where TRequestBuilder : IRequestParameterBuilder
        where TResponseParser : IResponseParser, new()
    {
        private const string ServerUrlDirectory = "/Microsoft-Server-ActiveSync";
        private const string ContentTypeWbXml = "application/vnd.ms-sync.wbxml";
        
        protected ASCommandBase(TRequestBuilder parameters, ASRequestSettings settings)
            : base(new ASRequestParameter<TRequestBuilder>(parameters, settings))
        {
        }

        public override async Task<ResponseResult<TResponseParser>> Execute()
        {
            var webRequestBuilder = new ASWebRequestBuilder();

            string xml = this.Parameters.BuildXml(this.CommandName);
            
            NetworkCredential credential = new NetworkCredential(this.Settings.Login, this.Settings.Password);

            string url = string.Format("{0}{1}?Cmd={2}&User={3}&DeviceId={4}&DeviceType={5}", 
                this.Settings.HostName, 
                ServerUrlDirectory,
                this.CommandName, 
                this.Settings.Login, 
                this.Settings.DeviceId, 
                this.Settings.DeviceType);

            var headers = new Dictionary<string, string>
            {
                { "MS-ASProtocolVersion", this.Settings.ProtocolVersion },
                { "X-MS-PolicyKey", this.Settings.PolicyKey.ToString() },
            };

            WebRequestResponse response = null;
            try
            {
                response = await webRequestBuilder.SendRequestAsync(
                    url, 
                    HttpMethod.Post, 
                    ContentTypeWbXml, 
                    xml, 
                    headers, 
                    credential);

                // settings must be updated if a redirection occured
                string requestUri = response.Request.RequestUri.GetSchemeAndHost();
                if (requestUri.TrimEnd('/') != this.Settings.HostName.TrimEnd('/'))
                {
                    string oldHostName = this.Settings.HostName;
                    this.Settings.HostName = requestUri;
                    LogService.Log("ASCommandBase", $"Successfull redirection from {oldHostName} to {requestUri}");
                }

                if (response.Response.IsSuccessStatusCode)
                {
                    var parser = new TResponseParser();
                    parser.ParseResponse(this.CommandName, response);

                    return ResponseResult<TResponseParser>.Create(parser, requestUri);
                }
                else
                {
                    return this.CreateResponseResult(null, response, requestUri);
                }
            }
            catch (Exception ex)
            {
                return this.CreateResponseResult(ex, response, url);
            }
        }        
    }
}
