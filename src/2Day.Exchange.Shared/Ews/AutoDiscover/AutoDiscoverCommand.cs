using Chartreuse.Today.Exchange.Ews.Commands;

namespace Chartreuse.Today.Exchange.Ews.AutoDiscover
{
    public class AutoDiscoverCommand : EwsCommandBase<AutoDiscoverParameter, AutoDiscoverResult>
    {
        public override string CommandName
        {
            get { return "AutoDiscover"; }
        }

        public AutoDiscoverCommand(AutoDiscoverParameter parameters, EwsRequestSettings settings) 
            : base(new EwsRequestParameter<AutoDiscoverParameter>(parameters, settings), new AutoDiscoverRequestBuilder())
        {
        }
    }
}