namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class UpdateItemsCommand : EwsCommandBase<UpdateItemsParameter, UpdateItemsResult>
    {
        public override string CommandName
        {
            get { return "UpdateItem"; }
        }

        public UpdateItemsCommand(UpdateItemsParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<UpdateItemsParameter>(parameters, settings))
        {
        }
    }
}
