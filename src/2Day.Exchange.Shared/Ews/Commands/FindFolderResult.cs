using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class FindFolderResult : EwsResponseParserBase
    {
        public List<EwsFolderIdentifier> Folders { get; private set; }

        public FindFolderResult()
        {
            this.Folders = new List<EwsFolderIdentifier>();
        }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            foreach (var responseMessage in responseMessages.Where(r => r.Class == EwsResponseClass.Success))
            {
                var content = responseMessage.Content;
                foreach (var item in content.XGetChildrenOf("RootFolder/t:Folders"))
                {
                    string id = item.XGetChildNodeAttributeValue<string>("FolderId", "Id");
                    string changeKey = item.XGetChildNodeAttributeValue<string>("FolderId", "ChangeKey");
                    string displayName = item.XGetChildValue<string>("DisplayName");
                    int totalCount = item.XGetChildValue<int>("TotalCount");
                    int childFolderCount = item.XGetChildValue<int>("ChildFolderCount");
                    int unreadCount = item.XGetChildValue<int>("UnreadCount");

                    this.Folders.Add(new EwsFolderIdentifier(id, changeKey, displayName, totalCount, childFolderCount, unreadCount));
                }
            }
        }
    }
    /*
        <m:FindFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
          <m:RootFolder TotalItemsInView="4" IncludesLastItemInRange="true">
            <t:Folders>
              <t:TasksFolder>
                <t:FolderId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQAuAAAAAABjivCyBkfSRJfnH5QjLXveAQCdlcnfVxEMSoRfenadZfEJAAAmtuR5AAA=" ChangeKey="BAAAABQAAABEXMWpNAzJSbsTkJdxbxe5AAAoJw==" />
                <t:DisplayName>childFolder </t:DisplayName>
                <t:TotalCount>0</t:TotalCount>
                <t:ChildFolderCount>0</t:ChildFolderCount>
                <t:UnreadCount>0</t:UnreadCount>
              </t:TasksFolder>
              <t:TasksFolder>
                <t:FolderId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQAuAAAAAABjivCyBkfSRJfnH5QjLXveAQCdlcnfVxEMSoRfenadZfEJAAAcKWu1AAA=" ChangeKey="BAAAABQAAABEXMWpNAzJSbsTkJdxbxe5AAAoGQ==" />
                <t:DisplayName>new folder</t:DisplayName>
                <t:TotalCount>1</t:TotalCount>
                <t:ChildFolderCount>0</t:ChildFolderCount>
                <t:UnreadCount>0</t:UnreadCount>
              </t:TasksFolder>
              <t:TasksFolder>
                <t:FolderId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQAuAAAAAABjivCyBkfSRJfnH5QjLXveAQCdlcnfVxEMSoRfenadZfEJAAAcKWu2AAA=" ChangeKey="BAAAABQAAABEXMWpNAzJSbsTkJdxbxe5AAAoFg==" />
                <t:DisplayName>sub folder</t:DisplayName>
                <t:TotalCount>0</t:TotalCount>
                <t:ChildFolderCount>0</t:ChildFolderCount>
                <t:UnreadCount>0</t:UnreadCount>
              </t:TasksFolder>
              <t:TasksFolder>
                <t:FolderId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQAuAAAAAABjivCyBkfSRJfnH5QjLXveAQCdlcnfVxEMSoRfenadZfEJAAAmtuR3AAA=" ChangeKey="BAAAABQAAABEXMWpNAzJSbsTkJdxbxe5AAAoIA==" />
                <t:DisplayName>tititest</t:DisplayName>
                <t:TotalCount>0</t:TotalCount>
                <t:ChildFolderCount>0</t:ChildFolderCount>
                <t:UnreadCount>0</t:UnreadCount>
              </t:TasksFolder>
            </t:Folders>
          </m:RootFolder>
        </m:FindFolderResponseMessage>
     */
}