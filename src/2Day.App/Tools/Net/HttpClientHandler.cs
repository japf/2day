using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Net;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpMethod = System.Net.Http.HttpMethod;
using HttpRequestMessage = System.Net.Http.HttpRequestMessage;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;

namespace Chartreuse.Today.App.Tools.Net
{
    /// <summary>
    /// This class fixes an issue that appeared in Windows 10 UWP apps using System.Net.Http.HttpClient (from NuGet package).
    /// In summary, if a POST request is sent and a redirection is needed, that will cause an exception. This scenario occurs
    /// when syncing with Exchange through EWS or with Outlook.com because of the redirect that happens at login.
    /// 
    /// See https://social.msdn.microsoft.com/Forums/en-US/9e137127-e0e5-4aec-a7a9-d66f5b84c70b/rtm-known-issue-systemnethttphttpclient-or-httpwebrequest-class-usage-in-a-uwp-app-throws-a?forum=Win10SDKToolsIssues
    /// 
    /// Fixes is coming in a future update of the Windows 10 UWP SDK
    /// See https://github.com/dotnet/corefx/commit/e298b31e1da3dea3c3fd78f5f713b4659136484e
    /// </summary>
    public class HttpClientHandler
    {
        public static void Setup()
        {
            WebRequestBuilder.CustomHttpClientFactory = () => new SystemWebHttpClientFactory();
        }

        private class SystemWebHttpClientFactory : IHttpClient
        {
            public async Task<SystemNetHttpRequestResult> SendRequestAsync(string url, HttpMethod method, string contentType, string requestBody, byte[] requestBytes, Dictionary<string, string> headers, NetworkCredential credentials)
            {
                var filter = new HttpBaseProtocolFilter
                {
                    AutomaticDecompression = true,
                    AllowAutoRedirect = true,
                    AllowUI = false                    
                };

                if (filter.IgnorableServerCertificateErrors != null)
                {
                    filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                    filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                }

                // use this to make sure we don't have weird behavior where the app caches credentials for way too long
                // the real fix seems to be using a new API introduced in 14295 but that's not the version that is targeted today
                // see: http://stackoverflow.com/questions/30731424/how-to-stop-credential-caching-on-windows-web-http-httpclient
                filter.CookieUsageBehavior = HttpCookieUsageBehavior.NoCookies;
                
                if (credentials != null)
                    filter.ServerCredential = new PasswordCredential("2Day", credentials.UserName, credentials.Password);
                else
                    filter.ServerCredential = null;

                var client = new HttpClient(filter);

                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                var request = new Windows.Web.Http.HttpRequestMessage(method.ToWindowsHttp(), SafeUri.Get(url));
                HttpRequestMessage systemNetRequest = request.ToSystemHttp();

                if (requestBody != null)
                {
                    if (method != HttpMethod.Post)
                        throw new NotSupportedException("Request body must use POST method");

                    var arrayContent = new HttpBufferContent(requestBytes.AsBuffer());

                    if (!string.IsNullOrWhiteSpace(contentType))
                        arrayContent.Headers.ContentType = new HttpMediaTypeHeaderValue(contentType);

                    if (method == HttpMethod.Post)
                        request.Content = arrayContent;
                }

                WebRequestBuilder.TraceRequest(systemNetRequest, requestBody);
                CancellationTokenSource timeoutCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                Windows.Web.Http.HttpResponseMessage response = await client.SendRequestAsync(request).AsTask(timeoutCancellationToken.Token);

                client.Dispose();

                HttpResponseMessage systemNetResponse = await response.ToWindowsHttp(systemNetRequest);

                return new SystemNetHttpRequestResult
                {
                    Request = systemNetRequest,
                    Response = systemNetResponse
                };
            }
        }
    }
}
