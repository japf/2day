namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteFolderCommand : EwsCommandBase<DeleteFolderParameter, DeleteFolderResult>
    {
        public override string CommandName
        {
            get { return "DeleteFolder"; }
        }

        public DeleteFolderCommand(DeleteFolderParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<DeleteFolderParameter>(parameters, settings))
        {
        }
    }
}