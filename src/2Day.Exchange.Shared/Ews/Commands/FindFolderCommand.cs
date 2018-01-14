namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class FindFolderCommand : EwsCommandBase<FindFolderParameter, FindFolderResult>
    {
        public override string CommandName
        {
            get { return "FindFolder"; }
        }

        public FindFolderCommand(FindFolderParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<FindFolderParameter>(parameters, settings))
        {
        }
    }    
}
