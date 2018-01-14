namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class GetFolderIdentifiersCommand :  EwsCommandBase<GetFolderIdentifiersParameter, GetFolderIdentifiersResult>
    {
        public override string CommandName
        {
            get { return "GetFolder"; }
        }

        public GetFolderIdentifiersCommand(GetFolderIdentifiersParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<GetFolderIdentifiersParameter>(parameters, settings))
        {
        }
    }
}
