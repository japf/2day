namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class GetItemCommand : EwsCommandBase<GetItemParameter, GetItemResult>
    {
        public override string CommandName
        {
            get { return "GetItem"; }
        }

        public GetItemCommand(GetItemParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<GetItemParameter>(parameters, settings))
        {
        }
    }    
}
