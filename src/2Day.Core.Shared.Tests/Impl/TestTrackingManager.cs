using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Tests.Impl
{
    public class TestTrackingManager : ITrackingManager
    {
        public void Trace(string message)
        {
        }

        public void Event(TrackingSource source, string category, string section)
        {
        }

        public void TagEvent(string name, Dictionary<string, string> attributes)
        {
        }

        public void Exception(Exception exception, string message = null, bool isFatal = false)
        {
        }

        public void Time(TimeSpan duration, string category, string variable, string label)
        {
        }
    }
}
