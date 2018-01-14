using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Tools.Net
{
    /// <summary>
    /// An helper class that exposes extension methods to convert System.Net.Http types into
    /// System.Web.Http and vice-versa
    /// </summary>
    public static class NetHttpInterop
    {
        public static async Task<HttpResponseMessage> ToWindowsHttp(this Windows.Web.Http.HttpResponseMessage response, HttpRequestMessage requestMessage)
        {
            var result = new HttpResponseMessage((HttpStatusCode)response.StatusCode);

            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                try
                {
                    result.Headers.Add(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, $"Unable to add header {header.Key} to System.Net.Http.HttpResponseMessage");
                }
            }

            result.RequestMessage = requestMessage;

            var buffer = await response.Content.ReadAsBufferAsync();
            result.Content = new ByteArrayContent(buffer.ToArray());

            return result;
        }

        public static HttpRequestMessage ToSystemHttp(this Windows.Web.Http.HttpRequestMessage request)
        {
            var result = new HttpRequestMessage(request.Method.ToSystemHttp(), request.RequestUri);

            return result;
        }

        public static HttpMethod ToSystemHttp(this Windows.Web.Http.HttpMethod method)
        {
            if (method == Windows.Web.Http.HttpMethod.Post)
                return HttpMethod.Post;
            else if (method == Windows.Web.Http.HttpMethod.Get)
                return HttpMethod.Get;
            else
                throw new NotSupportedException();
        }

        public static Windows.Web.Http.HttpMethod ToWindowsHttp(this HttpMethod method)
        {
            if (method == HttpMethod.Post)
                return Windows.Web.Http.HttpMethod.Post;
            else if (method == HttpMethod.Get)
                return Windows.Web.Http.HttpMethod.Get;
            else
                throw new NotSupportedException();
        }
    }
}