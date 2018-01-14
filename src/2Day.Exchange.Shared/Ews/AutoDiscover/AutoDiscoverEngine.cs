using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.ActiveSync.Exceptions;
using Chartreuse.Today.Exchange.Ews.Commands;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Exchange.Shared;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.Ews.AutoDiscover
{
    public class AutoDiscoverEngine
    {
        private const string Office365Endpoint = "https://outlook.office365.com/ews/Exchange.asmx";

        private static readonly List<string> ewsEndpoints;
        private static readonly List<string> autoDiscoverEndpoints;

        static AutoDiscoverEngine()
        {
            ewsEndpoints = new List<string>
            {
                "https://{0}/EWS/Exchange.asmx",
                "https://owa.{0}/EWS/Exchange.asmx",
                "https://ews.{0}/EWS/Exchange.asmx",
                "https://mail.{0}/EWS/Exchange.asmx",
                "https://exchange.{0}/EWS/Exchange.asmx"
            };

            autoDiscoverEndpoints = new List<string>
            {
                "https://autodiscover.{0}/autodiscover/autodiscover.xml",
	            "https://{0}/autodiscover/autodiscover.xml",
	            "http://autodiscover.{0}/autodiscover/autodiscover.xml",
	            "http://{0}/autodiscover/autodiscover.xml"
            };
        }

        public async Task<AutoDiscoverEngineResult> AutoDiscoverAsync(string email, string username, string password, ExchangeServerVersion version)
        {
            if (!email.Contains("@"))
                throw new ArgumentException("Invalid email format");

            var split = email.Split('@');
            if (split.Length != 2)
                throw new ArgumentException("Invalid email format");

            string domain = split[1];

            var result = await this.SmartGuessAsync(email, username, password, domain, version);
            if (result != null)
                return result;
            
            result = await this.EwsAutoDiscoverAsync(email, username, password, domain, version);

            return result;
        }

        private async Task<AutoDiscoverEngineResult> SmartGuessAsync(string email, string username, string password, string domain, ExchangeServerVersion version, string endpoint = null)
        {
            List<string> endpoints;
            if (endpoint == null)
            {
                endpoints = ewsEndpoints;
                if (version == ExchangeServerVersion.ExchangeOffice365)
                    endpoints.Insert(0, Office365Endpoint);
            }
            else
            {
                endpoints = new List<string> { endpoint };
            }

            var result = await this.ExecuteCommand(
                "smart guess",
                endpoints,
                domain,
                uri => new GetFolderIdentifiersCommand(new GetFolderIdentifiersParameter(), CreateRequestSettings(email, username, password, uri)));

            if (result != null && result.Data != null && !string.IsNullOrWhiteSpace(result.Uri))
            {
                var uri = TryCreateUri(result.Uri);
                if (uri != null)
                    return new AutoDiscoverEngineResult(uri, null);
            }

            return null;
        }

        private async Task<AutoDiscoverEngineResult> EwsAutoDiscoverAsync(string email, string username, string password, string domain, ExchangeServerVersion version)
        {
            var commandResult = await this.ExecuteCommand(
                "ews auto discover",
                autoDiscoverEndpoints,
                domain,
                uri => new AutoDiscoverCommand(new AutoDiscoverParameter(email), CreateRequestSettings(email, username, password, uri)));

            if (commandResult != null && commandResult.Data != null)
            {
                var data = commandResult.Data;

                // external
                if (!string.IsNullOrWhiteSpace(data.ExternalEwsUrl))
                {
                    LogService.Log("AutoDiscover", "Trying with external ews url: " + data.ExternalEwsUrl);
                    var result = await this.SmartGuessAsync(email, username, password, domain, version, data.ExternalEwsUrl);
                    if (result != null)
                        return result;
                }

                // internal
                if (!string.IsNullOrWhiteSpace(data.InternalEwsUrl))
                {
                    LogService.Log("AutoDiscover", "Trying with internal ews url: " + data.InternalEwsUrl);
                    var result = await this.SmartGuessAsync(email, username, password, domain, version, data.InternalEwsUrl);
                    if (result != null)
                        return result;
                }

                // redirect addr
                if (!string.IsNullOrWhiteSpace(data.RedirectEmailAddress))
                {
                    LogService.Log("AutoDiscover", "Trying with email redirect: " + data.RedirectEmailAddress);
                    var result = await this.SmartGuessAsync(data.RedirectEmailAddress, username, password, domain, version);
                    if (result != null)
                        return new AutoDiscoverEngineResult(result.ServerUri, data.RedirectEmailAddress);
                }
            }

            return null;
        }

        private async Task<ResponseResult<T>> ExecuteCommand<T>(string name, IEnumerable<string> endpoints, string replaceTemplate, Func<string, IWebCommandBase<T>> commandFactory) where T : IResponseParser
        {
            Log($"Starting {name}");

            ResponseResult<T> unauthorizedResult = null;

            foreach (var endpoint in endpoints)
            {
                string uri = string.Format(endpoint, replaceTemplate);
                Log($"Trying endpoint: {uri}");

                var command = commandFactory(uri);
                var result = await command.Execute();
                if (result.Error == null && result.Data != null)
                {
                    Log($"Endpoint fount at {uri}");
                    return result;
                }
                else if (result.Error is CommandException && ((CommandException)result.Error).Response != null && (((CommandException)result.Error).Response.StatusCode == HttpStatusCode.Unauthorized))
                {
                    Log($"Endpoint {uri} is unauthorized");
                    unauthorizedResult = result;
                }
                else
                {
                    Log($"Endpoint {uri} failed raison: {result.Error.Message}");
                }
            }

            if (unauthorizedResult != null)
            {
                Log($"Returning unauthorized endpoint: {unauthorizedResult.Uri}");
                return unauthorizedResult;
            }

            Log($"End {name}, all endpoints failed");

            return null;
        }

        private static EwsRequestSettings CreateRequestSettings(string email, string username, string password, string endpoint)
        {
            return new EwsRequestSettings(email, username, password, endpoint);
        }

        private static void Log(string message)
        {
            LogService.Log("AutoDiscoverEngine", message);
        }

        private static Uri TryCreateUri(string uri)
        {
            try
            {
                return SafeUri.Get(uri);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
