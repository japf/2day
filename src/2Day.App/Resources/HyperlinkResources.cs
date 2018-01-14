using Chartreuse.Today.Core.Shared;

namespace Chartreuse.Today.App.Shared.Resources
{
    public class HyperlinkResources
    {
        public static HyperlinkResources Instance = new HyperlinkResources();

        public string HelpPageChooseSyncAddress
        {
            get { return Constants.HelpPageChooseSyncAddress; }
        }

        public string HelpPageSmartViews 
        {
            get { return Constants.HelpPageSmartViewAddress; }
        }
    }
}
