using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Net
{
    public interface IHttpClient
    {
        Task<SystemNetHttpRequestResult> SendRequestAsync(string url, HttpMethod method, string contentType, string requestBody, byte[] requestBytes, Dictionary<string, string> headers, NetworkCredential credentials);
    }
}