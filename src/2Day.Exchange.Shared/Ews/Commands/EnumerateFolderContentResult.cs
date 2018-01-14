using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class EnumerateFolderContentResult : EwsResponseParserBase
    {
        public List<EwsItemIdentifier> ItemIdentifiers { get; private set; }

        public EnumerateFolderContentResult()
        {
            this.ItemIdentifiers = new List<EwsItemIdentifier>();            
        }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            foreach (var responseMessage in responseMessages.Select(m => m.Content))
            {
                foreach (var item in responseMessage.XGetChildrenOf("RootFolder/Items"))
                {
                    var id = item.XGetChildNodeAttributeValue<string>("ItemId", "Id");
                    var changeKey = item.XGetChildNodeAttributeValue<string>("ItemId", "ChangeKey");
                    var parentFolderId = item.XGetChildNodeAttributeValue<string>("ParentFolderId", "Id");
                    var parentFolderChangeKey = item.XGetChildNodeAttributeValue<string>("ParentFolderId", "ChangeKey");

                    this.ItemIdentifiers.Add(new EwsItemIdentifier(id, changeKey)
                    {
                        ParentFolderId = parentFolderId,
                        ParentFolderChangeKey = parentFolderChangeKey
                    });
                }
            }
        }
    }
}
/*
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Header>
    <ServerVersionInfo MajorVersion="15" MinorVersion="0" MajorBuildNumber="1039" MinorBuildNumber="11" xmlns:h="http://schemas.microsoft.com/exchange/services/2006/types" xmlns="http://schemas.microsoft.com/exchange/services/2006/types" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
  </s:Header>
  <s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <m:FindItemResponse xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages" xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
      <m:ResponseMessages>
        <m:FindItemResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
          <m:RootFolder IndexedPagingOffset="4" TotalItemsInView="4" IncludesLastItemInRange="true">
            <t:Items>
              <t:Task>
                <t:ItemId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQBGAAAAAABjivCyBkfSRJfnH5QjLXveBwCdlcnfVxEMSoRfenadZfEJAAAAAAESAACdlcnfVxEMSoRfenadZfEJAAAT53vSAAA=" ChangeKey="EwAAABYAAACdlcnfVxEMSoRfenadZfEJAAAT51JN" />
                <t:ParentFolderId Id="AQAkAGplcmVteS5hbGwAZXNAdmVyY29ycy5vbm1pY3Jvc29mdC5jb20ALgAAA2OK8LIGR9JEl+cflCMte94BAJ2Vyd9XEQxKhF96dp1l8QkAAAIBEgAAAA==" ChangeKey="AQAAAA==" />
              </t:Task>
              <t:Task>
                <t:ItemId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQBGAAAAAABjivCyBkfSRJfnH5QjLXveBwCdlcnfVxEMSoRfenadZfEJAAAAAAESAACdlcnfVxEMSoRfenadZfEJAAAT53vRAAA=" ChangeKey="EwAAABYAAACdlcnfVxEMSoRfenadZfEJAAAT51JJ" />
                <t:ParentFolderId Id="AQAkAGplcmVteS5hbGwAZXNAdmVyY29ycy5vbm1pY3Jvc29mdC5jb20ALgAAA2OK8LIGR9JEl+cflCMte94BAJ2Vyd9XEQxKhF96dp1l8QkAAAIBEgAAAA==" ChangeKey="AQAAAA==" />
              </t:Task>
            </t:Items>
          </m:RootFolder>
        </m:FindItemResponseMessage>
      </m:ResponseMessages>
    </m:FindItemResponse>
  </s:Body>
</s:Envelope> 
 */
