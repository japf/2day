namespace Chartreuse.Today.Exchange.Ews.Model
{
    public class EwsItemIdentifier : EwsItemIdentifierBase
    {
        public EwsItemIdentifier(string id, string changeKey) : base(id, changeKey)
        {
        }

        public EwsItemIdentifier(string errorMessage) : base(errorMessage)
        {
        }

        public string ParentFolderId { get; set; }
        public string ParentFolderChangeKey { get; set; }
    }
}