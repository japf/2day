using System;

namespace Chartreuse.Today.Exchange.Ews.AutoDiscover
{
    public class AutoDiscoverEngineResult
    {
        public Uri ServerUri { get; private set; }
        public string RedirectEmail { get; private set; }
        
        public AutoDiscoverEngineResult(Uri serverUri, string redirectEmail)
        {
            this.RedirectEmail = redirectEmail;
            this.ServerUri = serverUri;
        }
    }
}