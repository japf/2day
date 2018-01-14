using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;

namespace Chartreuse.Today.Exchange.Ews.AutoDiscover
{
    internal class AutoDiscoverRequestBuilder : WebRequestBuilder
    {
        protected override byte[] PrepareRequestBody(string content)
        {
            return Encoding.UTF8.GetBytes(content);
        }

        protected override async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}