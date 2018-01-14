using Chartreuse.Today.Exchange.Ews.Schema;

namespace Chartreuse.Today.Exchange.Ews.Model
{
    public class EwsFolderIdentifier : EwsItemIdentifierBase
    {
        public EwsFolderIdentifier(string id, string changeKey)
            : base(id, changeKey)
        {
        }

        public EwsFolderIdentifier(string id, bool isDistinguishedFolderId)
            : base(id, "none")
        {
            this.IsDistinguishedFolderId = isDistinguishedFolderId;
        }

        public EwsFolderIdentifier(string id, string changeKey, string displayName, int totalCount, int childFolderCount, int unreadCount) 
            : base(id, changeKey)
        {
            this.DisplayName = displayName;
            this.TotalCount = totalCount;
            this.ChildFolderCount = childFolderCount;
            this.UnreadCount = unreadCount;
        }

        public EwsFolderIdentifier(string errorMessage)
            : base(errorMessage)
        {
        }

        public string DisplayName { get; private set; }

        public bool IsDistinguishedFolderId { get; protected set; }

        public int TotalCount { get; private set; }
        public int ChildFolderCount { get; private set; }
        public int UnreadCount { get; private set; }

        public string GetXml()
        {
            return EwsTaskSchema.GetXmlForFolderIdentifier(this);
        }
    }
}