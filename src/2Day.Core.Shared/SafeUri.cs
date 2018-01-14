using System;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared
{
    public static class SafeUri
    {

        public static Uri Get(string uri)
        {
            try
            {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (string.IsNullOrWhiteSpace(uri))
                    throw new ArgumentNullException(nameof(uri));

                return new Uri(uri);
            }
            catch (Exception ex)
            {
                string strippedUri = uri;
                if (uri != null && uri.Length > 500)
                    strippedUri = uri.Substring(0, 500);

                int length = 0;
                if (uri != null)
                    length = uri.Length;

                TrackingManagerHelper.Exception(ex, $"Failed to created uri length {length} from: {strippedUri}");
                throw new ArgumentException($"Failed to created uri length {length}  from: {strippedUri}");
            }
        }

        public static Uri Get(string uri, UriKind uriKind)
        {
            try
            {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (string.IsNullOrWhiteSpace(uri))
                    throw new ArgumentNullException(nameof(uri));

                return new Uri(uri, uriKind);
            }
            catch (Exception ex)
            {
                string strippedUri = uri;
                if (uri != null && uri.Length > 500)
                    strippedUri = uri.Substring(0, 500);

                int length = 0;
                if (uri != null)
                    length = uri.Length;

                TrackingManagerHelper.Exception(ex, $"Failed to created uri length {length} from: {strippedUri}");
                throw new ArgumentException($"Failed to created uri length {length} from: {strippedUri}");
            }
        }
    }
}
