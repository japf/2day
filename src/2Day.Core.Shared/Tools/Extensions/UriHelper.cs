using System;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    public static class UriHelper
    {
        public static string GetSchemeAndHost(this Uri uri)
        {
            string requestUri = null;
            try
            {
                if (!uri.IsAbsoluteUri)
                    uri = SafeUri.Get("https://" + uri.OriginalString);

                requestUri = uri.TryGetSchemeAndHost();
            }
            catch (Exception)
            {
                if (Ioc.HasType<ITrackingManager>())
                    Ioc.Resolve<ITrackingManager>().Event(TrackingSource.Misc, "UriHelper", string.Format("Could not parse: {0}", requestUri));

                requestUri = uri.ToString();
            }

            return requestUri;
        }

        private static string TryGetSchemeAndHost(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            return uri.Scheme + "://" + uri.Host;
        }
    }
}
