namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class GetFolderIdentifiersParameter : EwsRequestParameterBuilderBase
    {
        protected override void EnsureCanExecute()
        {
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <FolderShape>" +
                "       <t:BaseShape>Default</t:BaseShape>" +
                "   </FolderShape>" +
                "   <FolderIds>" +
                "       <t:DistinguishedFolderId Id='tasks' />" +
                "       <t:DistinguishedFolderId Id='deleteditems' />" +
                "       <t:DistinguishedFolderId Id='drafts' />" +
                "       <t:DistinguishedFolderId Id='inbox' />" +
                "   </FolderIds>";
            return command;
        }
    }
}
