using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Ews.Schema;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class GetItemParameter : EwsRequestParameterBuilderBase
    {
        private readonly EwsItemType ewsItemType;
        public List<EwsItemIdentifier> ItemIdentifiers { get; private set; }

        public GetItemParameter(EwsItemType ewsItemType)
        {
            this.ewsItemType = ewsItemType;
            this.ItemIdentifiers = new List<EwsItemIdentifier>();
        }

        protected override void EnsureCanExecute()
        {
            if (this.ItemIdentifiers.Count == 0)
                throw new CommandCannotExecuteException("Command cannot execute with no item identifiers");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <ItemShape>" +
                "      <t:BaseShape>IdOnly</t:BaseShape>" +
                "      <t:BodyType>Best</t:BodyType>" +
                "      <t:AdditionalProperties>" +
                "          {0}" +
                "      </t:AdditionalProperties>" +
                "   </ItemShape>" +
                "   <ItemIds>" +
                "      {1}" +
                "   </ItemIds>";

            const string itemTemplate = "<t:ItemId Id='{0}' ChangeKey='{1}' />";
            StringBuilder builder = new StringBuilder();
            foreach (var itemIdentifier in this.ItemIdentifiers)
            {
                builder.AppendLine(string.Format(itemTemplate, itemIdentifier.Id, itemIdentifier.ChangeKey));
            }

            string xmlCommand = string.Format(
                command,
                EwsTaskSchema.GetAllFieldXml(this.ewsItemType),
                builder);

            return xmlCommand;
        }
    }
}