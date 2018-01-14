namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class MoveItemsCommand : EwsCommandBase<MoveItemsParameter, MoveItemsResult>
    {
        public override string CommandName
        {
            get { return "MoveItem"; }
        }

        public MoveItemsCommand(MoveItemsParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<MoveItemsParameter>(parameters, settings))
        {
        }
    }
}
