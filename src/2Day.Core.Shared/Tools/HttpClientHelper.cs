using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public static class HttpClientHelper
    {
        public const string JsonContentType = "application/json";

        public static HttpClient GetJsonHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));

            return client;
        }

        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string endpointUri, string json)
        {
            return client.PostAsync(endpointUri, new StringContent(json, Encoding.UTF8, JsonContentType));

        }
    }
}
