namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteHardItemsCommand : EwsCommandBase<DeleteHardItemsParameter, DeleteHardItemsResult>
    {
        public override string CommandName
        {
            get { return "DeleteItem"; }
        }
        
        public DeleteHardItemsCommand(DeleteHardItemsParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<DeleteHardItemsParameter>(parameters, settings))
        {
        }
    }
}