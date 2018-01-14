using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteFolderResult : EwsResponseParserBase
    {
        public List<EwsResponseClass> Classes { get; private set; }

        public DeleteFolderResult()
        {
            this.Classes = new List<EwsResponseClass>();
        }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            foreach (var responseMessage in responseMessages)
            {
                this.Classes.Add(responseMessage.Content.XGetAttributeValue<EwsResponseClass>("ResponseClass"));
            }
        }
    }
    /*
    <m:DeleteFolderResponse xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages" xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
      <m:ResponseMessages>
        <m:DeleteFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
        </m:DeleteFolderResponseMessage>
        <m:DeleteFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
        </m:DeleteFolderResponseMessage>
        <m:DeleteFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
        </m:DeleteFolderResponseMessage>
        <m:DeleteFolderResponseMessage ResponseClass="Success">
          <m:ResponseCode>NoError</m:ResponseCode>
        </m:DeleteFolderResponseMessage>
      </m:ResponseMessages>
    </m:DeleteFolderResponse>
     */
}