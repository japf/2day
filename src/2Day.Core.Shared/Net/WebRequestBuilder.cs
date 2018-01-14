using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Newtonsoft.Json;

namespace Chartreuse.Today.Core.Shared.Net
{
    public abstract class WebRequestBuilder
    {
        private static readonly Dictionary<string, string> EmptyHeaders = new Dictionary<string, string>();

        public static Func<IHttpClient> CustomHttpClientFactory { get; set; }

        public static event EventHandler<WebRequestInterceptorEventArgs> InterceptRequest;

        public async Task<TOutput> PostJsonAsync<TInput, TOutput>(string uri, TInput body) where TOutput : class
        {
            var content = await this.SendRequestAsync(uri, HttpMethod.Post, "application/json", JsonConvert.SerializeObject(body), EmptyHeaders, null);
            string json = content.ResponseBody;

            return JsonConvert.DeserializeObject<TOutput>(json);
        }

        public async Task<string> GetAsync(string uri)
        {
            var content = await this.SendRequestAsync(uri, HttpMethod.Get, null, null, null, null);

            return content.ResponseBody;
        }

        public async Task<WebRequestResponse> SendRequestAsync(string url, HttpMethod method, string contentType, string requestBody, Dictionary<string, string> headers, NetworkCredential credentials)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            if (method != HttpMethod.Post && method != HttpMethod.Get)
                throw new NotSupportedException("Only POST and GET methods are supported");
            if (method == HttpMethod.Post && requestBody == null)
                throw new NotSupportedException("Http POST method must have a body");

            HttpRequestMessage request = null;
            HttpResponseMessage response = null;

            if (InterceptRequest != null)
            {
                var requestInterceptor = new WebRequestInterceptorEventArgs() { RequestBody = requestBody };
                InterceptRequest(this, requestInterceptor);
                requestBody = requestInterceptor.RequestBody;
            }

            if (CustomHttpClientFactory != null)
            {
                var client = CustomHttpClientFactory();
                byte[] requestBytes = requestBody != null ? this.PrepareRequestBody(requestBody) : null;
                var result = await client.SendRequestAsync(url, method, contentType, requestBody, requestBytes, headers, credentials);

                request = result.Request;
                response = result.Response;
            }
            else
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                handler.Credentials = credentials;
                handler.AllowAutoRedirect = true;

                var currentClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

                if (headers != null)
                {
                    foreach (var header in headers)
                        currentClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                request = new HttpRequestMessage(method, url);

                if (requestBody != null)
                {
                    if (method != HttpMethod.Post)
                        throw new NotSupportedException("Request body must use POST method");

                    byte[] content = this.PrepareRequestBody(requestBody);
                    var arrayContent = new ByteArrayContent(content);

                    if (!string.IsNullOrWhiteSpace(contentType))
                        arrayContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    if (method == HttpMethod.Post)
                        request.Content = arrayContent;
                }

                TraceRequest(request, requestBody);
                response = await currentClient.SendAsync(request);

                currentClient.Dispose();
            }

            if (!response.IsSuccessStatusCode)
            {
                string redirect = this.CheckRedirection(url, request, response);
                if (!String.IsNullOrEmpty(redirect))
                {
                    WriteTrace("\n\n >> Redirection to " + redirect + "\n\n");
                    return await this.SendRequestAsync(redirect, method, contentType, requestBody, headers, credentials);
                }
                else
                {
                    string responseBody = await this.ReadResponseAsync(response);
                    string xml = "<empty response>";
                    if (!string.IsNullOrWhiteSpace(responseBody))
                        xml = FormatXml(responseBody);

                    WriteTrace("\n\n >> Request failed, response: " + xml + "\n\n");

                    return new WebRequestResponse(request, response, requestBody, responseBody);
                }
            }
            else
            {
                string responseBody = await this.ReadResponseAsync(response);
                TraceResponse(response, responseBody);

                return new WebRequestResponse(request, response, requestBody, responseBody);
            }
        }
        
        protected abstract byte[] PrepareRequestBody(string content);

        protected abstract Task<string> ReadResponseAsync(HttpResponseMessage response);

        protected virtual string CheckRedirection(string url, HttpRequestMessage request, HttpResponseMessage responseMessage)
        {
            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.Found:
                case HttpStatusCode.RedirectMethod:
                case HttpStatusCode.RedirectKeepVerb:
                    if (responseMessage.Headers.Location != null && responseMessage.Headers.Location.IsAbsoluteUri && responseMessage.Headers.Location.AbsoluteUri != null)
                        return responseMessage.Headers.Location.AbsoluteUri;
                    break;
                default:
                    if (responseMessage.RequestMessage.RequestUri != null && url != responseMessage.RequestMessage.RequestUri.AbsoluteUri)
                        return responseMessage.RequestMessage.RequestUri.AbsoluteUri;
                    break;
            }

            return null;
        }

        public static void WriteTrace(string trace)
        {
            LogService.Log(LogLevel.Network, "WebRequest", trace);
        }

        [DebuggerStepThrough]
        public static string FormatXml(string xml)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }

        public static void TraceRequest(HttpRequestMessage request, string content)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} HttpRequestMessage [", DateTime.Now.ToString("T"));
            builder.AppendLine();
            builder.AppendFormat(" Uri: {0}\n", request.RequestUri);
            builder.AppendFormat(" Method: {0}\n", request.Method);

            if (request.Content != null)
            {
                builder.Append(" Headers:\n");
                foreach (var header in request.Content.Headers)
                    builder.AppendFormat("  {0} = {1}\n", header.Key, header.Value.AggregateString());
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                builder.AppendLine(" Data:");
                builder.Append(FormatXml(content));
                builder.AppendLine();
            }

            builder.AppendLine("]");
            builder.AppendLine();

            string trace = builder.ToString();
            WriteTrace(trace);
        }

        [DebuggerStepThrough]
        public static void TraceResponse(HttpResponseMessage response, string responseContent)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} HttpResponseMessage [", DateTime.Now.ToString("T"));
            builder.AppendLine();
            builder.AppendFormat(" Uri: {0}\n", response.RequestMessage.RequestUri.ToString());
            builder.AppendFormat(" Method: {0}\n", response.RequestMessage.Method);

            builder.Append(" Headers:\n");
            foreach (var header in response.Content.Headers)
                builder.AppendFormat("  {0} = {1}\n", header.Key, header.Value.AggregateString());
            builder.AppendLine(" Data:");
            builder.Append(FormatXml(responseContent));
            builder.AppendLine();
            builder.AppendLine("]");
            builder.AppendLine();

            string trace = builder.ToString();
            WriteTrace(trace);
        }
    }
}
