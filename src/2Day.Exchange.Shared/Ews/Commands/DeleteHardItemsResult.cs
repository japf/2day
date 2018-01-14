using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteHardItemsResult : EwsResponseParserBase
    {
        public int DeletedSuccessCount { get; private set; }
        public int DeletedWarningCount { get; private set; }
        public int DeletedErrorCount { get; private set; }
        
        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            this.DeletedSuccessCount = responseMessages.Count(m => m.Class == EwsResponseClass.Success);
            this.DeletedWarningCount = responseMessages.Count(m => m.Class == EwsResponseClass.Warning);
            this.DeletedErrorCount = responseMessages.Count(m => m.Class == EwsResponseClass.Error);
        }
    }

    /*
        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
          <s:Header>
            <h:ServerVersionInfo MajorVersion="15" MinorVersion="1" MajorBuildNumber="609" MinorBuildNumber="16" xmlns:h="http://schemas.microsoft.com/exchange/services/2006/types" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
          </s:Header>
          <s:Body>
            <m:DeleteItemResponse xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
              <m:ResponseMessages>
                <m:DeleteItemResponseMessage ResponseClass="Success">
                  <m:ResponseCode>NoError</m:ResponseCode>
                </m:DeleteItemResponseMessage>
                <m:DeleteItemResponseMessage ResponseClass="Success">
                  <m:ResponseCode>NoError</m:ResponseCode>
                </m:DeleteItemResponseMessage> 
     */
}
