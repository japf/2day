using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateFolderResult : EwsResponseParserBase
    {
        public List<EwsFolderIdentifier> Identifiers { get; private set; }

        public CreateFolderResult()
        {
            this.Identifiers = new List<EwsFolderIdentifier>();
        }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            foreach (var responseMessage in responseMessages)
            {
                if (responseMessage.Class == EwsResponseClass.Success)
                {
                    foreach (var item in responseMessage.Content.XGetChildrenOf("Folders", "m"))
                    {
                        var id = item.XGetChildNodeAttributeValue<string>("FolderId", "Id");
                        var changeKey = item.XGetChildNodeAttributeValue<string>("FolderId", "ChangeKey");

                        this.Identifiers.Add(new EwsFolderIdentifier(id, changeKey));
                    }
                }
                else
                {
                    string message = "no message";
                    if (!string.IsNullOrEmpty(responseMessage.Message))
                        message = responseMessage.Message;

                    this.Identifiers.Add(new EwsFolderIdentifier(message));
                }
            }
        }
    }
    /*
    <m:CreateFolderResponse xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages" xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
      <m:ResponseMessages>
        <m:CreateFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
          <m:Folders>
            <t:TasksFolder>
              <t:FolderId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQAuAAAAAABjivCyBkfSRJfnH5QjLXveAQCdlcnfVxEMSoRfenadZfEJAAAmtuR6AAA=" ChangeKey="BAAAABYAAACdlcnfVxEMSoRfenadZfEJAAAmtbcI" />
            </t:TasksFolder>
          </m:Folders>
        </m:CreateFolderResponseMessage>
        <m:CreateFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
          <m:Folders>
            <t:TasksFolder>
              <t:FolderId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQAuAAAAAABjivCyBkfSRJfnH5QjLXveAQCdlcnfVxEMSoRfenadZfEJAAAmtuR7AAA=" ChangeKey="BAAAABYAAACdlcnfVxEMSoRfenadZfEJAAAmtbcN" />
            </t:TasksFolder>
          </m:Folders>
        </m:CreateFolderResponseMessage>
      </m:ResponseMessages>
    </m:CreateFolderResponse>
     */
}