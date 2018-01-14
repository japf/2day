using System;

namespace Chartreuse.Today.Core.Shared.Net
{
    public class WebRequestInterceptorEventArgs : EventArgs
    {
        public string RequestBody { get; set; }
    }
}