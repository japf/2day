using Chartreuse.Today.App.Shared.Resources;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.App.Resources
{
    public class ApplicationResources
    {
        public static ApplicationResources Instance = new ApplicationResources();

        private static readonly StringResourcesAccessor stringsAccessor = new StringResourcesAccessor();

        private static readonly StringResources stringResources = new StringResources();

        public HyperlinkResources HyperlinkResources { get { return HyperlinkResources.Instance; } }

        public StringResources SharedStrings { get { return stringResources; } }

        public StringResourcesAccessor StringResources { get { return stringsAccessor; } }
    }
}
