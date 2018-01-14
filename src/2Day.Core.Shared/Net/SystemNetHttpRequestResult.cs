using System.Net.Http;

namespace Chartreuse.Today.Core.Shared.Net
{
    public struct SystemNetHttpRequestResult
    {
        public HttpRequestMessage Request { get; set; }
        public HttpResponseMessage Response { get; set; }
    }
}