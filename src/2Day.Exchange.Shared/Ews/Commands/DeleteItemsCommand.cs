namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteItemsCommand : MoveItemsCommand
    {
        public DeleteItemsCommand(DeleteItemsParameter parameters, EwsRequestSettings settings)
            : base(parameters, settings)
        {
        }
    }
}