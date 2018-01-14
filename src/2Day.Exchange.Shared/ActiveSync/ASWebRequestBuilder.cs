using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    internal class ASWebRequestBuilder : WebRequestBuilder
    {
        protected override byte[] PrepareRequestBody(string content)
        {
            return ASWebXmlHelper.Encode(content);
        }

        protected override async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {            
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            return ASWebXmlHelper.Decode(byteArray);
        }

        protected override string CheckRedirection(string url, HttpRequestMessage request, HttpResponseMessage response)
        {
            IEnumerable<string> values;
            if ((int)response.StatusCode == 451 && response.Headers.TryGetValues("X-MS-Location", out values))
            {
                var redirects = new List<string>(values);
                if (redirects.Count > 0)
                {
                    Uri oldUri = response.RequestMessage.RequestUri;
                    string newHost = redirects[0];

                    return newHost + oldUri.Query;
                }
            }

            return null;
        }
    }
}
