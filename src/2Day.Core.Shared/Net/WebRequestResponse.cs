using System.Net.Http;

namespace Chartreuse.Today.Core.Shared.Net
{
    public class WebRequestResponse
    {
        private readonly HttpRequestMessage request;
        private readonly HttpResponseMessage response;
        private readonly string responseBody;
        private readonly string requestBody;

        public HttpRequestMessage Request
        {
            get { return this.request; }
        }

        public HttpResponseMessage Response
        {
            get { return this.response; }
        }

        public string RequestBody
        {
            get { return this.requestBody; }
        }

        public string ResponseBody
        {
            get { return this.responseBody; }
        }

        public WebRequestResponse(HttpRequestMessage request, HttpResponseMessage response, string requestBody, string responseBody)
        {
            this.request = request;
            this.response = response;
            this.requestBody = requestBody;
            this.responseBody = responseBody;
        }
    }
}