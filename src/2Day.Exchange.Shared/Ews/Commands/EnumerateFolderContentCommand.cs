namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class EnumerateFolderContentCommand : EwsCommandBase<EnumerateFolderContentParameter, EnumerateFolderContentResult>
    {
        public override string CommandName
        {
            get { return "FindItem"; }
        }

        public EnumerateFolderContentCommand(EnumerateFolderContentParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<EnumerateFolderContentParameter>(parameters, settings))
        {
        }
    }
}
