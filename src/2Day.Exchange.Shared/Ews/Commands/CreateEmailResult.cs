using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateEmailResult : EwsResponseParserBase
    {
        public List<EwsItemIdentifier> Identifiers { get; private set; }

        public CreateEmailResult()
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
                        string id = item.XGetChildNodeAttributeValue<string>("ItemId", "Id");
                        string changeKey = item.XGetChildNodeAttributeValue<string>("ItemId", "ChangeKey");

                        if (!string.IsNullOrWhiteSpace(changeKey))
                            this.Identifiers.Add(new EwsItemIdentifier(id, changeKey));
                        else
                            this.Identifiers.Add(new EwsItemIdentifier(id, "changeKey"));
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