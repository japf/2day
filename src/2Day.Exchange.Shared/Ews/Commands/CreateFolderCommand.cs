namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateFolderCommand : EwsCommandBase<CreateFolderParameter, CreateFolderResult>
    {
        public override string CommandName
        {
            get { return "CreateFolder"; }
        }

        public CreateFolderCommand(CreateFolderParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<CreateFolderParameter>(parameters, settings))
        {
        }
    }
}