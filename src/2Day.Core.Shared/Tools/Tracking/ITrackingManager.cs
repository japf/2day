using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Tools.Tracking
{
    public interface ITrackingManager
    {
        void Trace(string message);

        void Event(TrackingSource source, string category, string section);

        void TagEvent(string name, Dictionary<string, string> attributes);

        void Exception(Exception exception, string message = null, bool isFatal = false);
    }
}