using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class UpdateItemsResult : EwsResponseParserBase
    {
        public List<EwsItemIdentifier> Identifiers { get; private set; }

        public UpdateItemsResult()
        {
            this.Identifiers = new List<EwsItemIdentifier>();
        }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            foreach (var responseMessage in responseMessages)
            {
                if (responseMessage.Class == EwsResponseClass.Success)
                {
                    foreach (var item in responseMessage.Content.XGetChildrenOf("Items", "m"))
                    {
                        var id = item.XGetChildNodeAttributeValue<string>("ItemId", "Id");
                        var changeKey = item.XGetChildNodeAttributeValue<string>("ItemId", "ChangeKey");

                        this.Identifiers.Add(new EwsItemIdentifier(id, changeKey));
                    }
                }
                else
                {
                    this.Identifiers.Add(new EwsItemIdentifier(responseMessage.Message));
                }
            }
        }
    }
}

/*
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Header>
    <ServerVersionInfo MajorVersion="15" MinorVersion="0" MajorBuildNumber="1039" MinorBuildNumber="20" xmlns:h="http://schemas.microsoft.com/exchange/services/2006/types" xmlns="http://schemas.microsoft.com/exchange/services/2006/types" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
  </s:Header>
  <s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <m:UpdateItemResponse xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages" xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
      <m:ResponseMessages>
        <m:UpdateItemResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
          <m:Items>
            <t:Task>
              <t:ItemId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQBGAAAAAABjivCyBkfSRJfnH5QjLXveBwCdlcnfVxEMSoRfenadZfEJAAAAAAESAACdlcnfVxEMSoRfenadZfEJAAAcKFhSAAA=" ChangeKey="EwAAABYAAACdlcnfVxEMSoRfenadZfEJAAAcKFKc" />
            </t:Task>
          </m:Items>
        </m:UpdateItemResponseMessage>
      </m:ResponseMessages>
    </m:UpdateItemResponse>
  </s:Body>
</s:Envelope>
*/