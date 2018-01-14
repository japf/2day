using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Phone.Tools.Tracking;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestTrackingManager : ITrackingManager
    {
        public void StartSession()
        {
        }

        public void Log(TrackingLevel level, string message, Exception exception = null)
        {
        }

        public void Trace(string message)
        {
        }

        public void Event(TrackingSource source, string category, string section)
        {
        }

        public void TagEvent(string name, Dictionary<string,string> attributes)
        {
        }

        public void Exception(Exception exception, string message = null, bool isFatal = false)
        {
        }

        public void TrackPage(string pageName)
        {
        }

        public void Exception(Exception exception, bool isFatal, string message = null)
        {
        }        
    }
}