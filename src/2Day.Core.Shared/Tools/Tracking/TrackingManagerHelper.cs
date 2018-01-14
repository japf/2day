using System;

namespace Chartreuse.Today.Core.Shared.Tools.Tracking
{
    public static class TrackingManagerHelper
    {
        public static void Exception(Exception exception, string message = null, bool isFatal = false)
        {
            if (Ioc.HasType<ITrackingManager>())
            {
                ITrackingManager trackingManager = Ioc.Resolve<ITrackingManager>();
                trackingManager.Exception(exception, message, isFatal);
            }
        }

        public static void Trace(string message)
        {
            if (Ioc.HasType<ITrackingManager>())
            {
                ITrackingManager trackingManager = Ioc.Resolve<ITrackingManager>();
                trackingManager.Trace(message);
            }
        }
    }
}
