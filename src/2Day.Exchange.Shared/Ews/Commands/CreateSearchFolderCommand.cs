namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateSearchFolderCommand : EwsCommandBase<CreateSearchFolderParameter, CreateFolderResult>
    {
        public override string CommandName
        {
            get { return "CreateFolder"; }
        }

        public CreateSearchFolderCommand(CreateSearchFolderParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<CreateSearchFolderParameter>(parameters, settings))
        {
        }
    }
}