using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class GetFolderIdentifiersResult : EwsResponseParserBase
    {
        public EwsFolderIdentifier TaskFolderIdentifier { get; private set; }

        public EwsFolderIdentifier DeletedItemsFolderIdentifier { get; private set; }

        public EwsFolderIdentifier DraftItemsFolderIdentifier { get; private set; }

        public EwsFolderIdentifier InboxFolderIdentifier { get; private set; }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            var identifiers = new List<EwsFolderIdentifier>();

            foreach (var responseMessage in responseMessages.Select(m => m.Content))
           {
                foreach (var folder in responseMessage.XGetChildrenOf("Folders"))
                {
                    string id = folder.XGetChildNodeAttributeValue<string>("FolderId", "Id");
                    string changeKey = folder.XGetChildNodeAttributeValue<string>("FolderId", "ChangeKey");
                    string displayName = folder.XGetChildValue<string>("DisplayName");
                    int totalCount = folder.XGetChildValue<int>("TotalCount");
                    int childFolderCount = folder.XGetChildValue<int>("ChildFolderCount");
                    int unreadCount = folder.XGetChildValue<int>("UnreadCount");

                    identifiers.Add(new EwsFolderIdentifier(id, changeKey, displayName, totalCount, childFolderCount, unreadCount));
                }
            }

            if (identifiers.Count == 0)
                throw new CommandResponseException("No folder identifiers were found");

            this.TaskFolderIdentifier = identifiers[0];

            if (identifiers.Count > 0)
                this.DeletedItemsFolderIdentifier = identifiers[1];

            if (identifiers.Count > 1)
                this.DraftItemsFolderIdentifier = identifiers[2];

            if (identifiers.Count > 2)
                this.InboxFolderIdentifier = identifiers[3];
        }
    }
}