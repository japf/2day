using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test
{
    [TestClass]
    public class WebRequestBuilderTest
    {
        public async Task Test()
        {
            // unable to reproduce the problem where res.Request.RequestUri contains a non absolute uri !
            var req = new RequestBuilder();
            var res = await req.SendRequestAsync("htps://snt-m.hotmail.com/dskodkosk", HttpMethod.Get, "text/html", null, new Dictionary<string, string>(), null);

            var t = res.Response.StatusCode;
            var redir = res.Request.RequestUri.GetSchemeAndHost();
        }

        private class RequestBuilder : WebRequestBuilder
        {
            protected override byte[] PrepareRequestBody(string content)
            {
                if (string.IsNullOrEmpty(content))
                    return new byte[] { };

                return Encoding.UTF8.GetBytes(content);
            }

            protected override async Task<string> ReadResponseAsync(HttpResponseMessage response)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
