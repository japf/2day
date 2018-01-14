namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateItemsCommand : EwsCommandBase<CreateItemsParameter, CreateItemsResult>
    {
        public override string CommandName
        {
            get { return "CreateItem"; }
        }

        public CreateItemsCommand(CreateItemsParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<CreateItemsParameter>(parameters, settings))
        {
        }
    }
}
