using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteHardItemsParameter : EwsRequestParameterBuilderBase
    {
        public List<EwsItemIdentifier> Identifiers { get; }

        protected override Dictionary<string, string> GetCommandAttributes()
        {
            return new Dictionary<string, string>
            {
                { "DeleteType", "HardDelete" },
                { "AffectedTaskOccurrences", "AllOccurrences" }
            };
        }

        public DeleteHardItemsParameter()
        {
            this.Identifiers = new List<EwsItemIdentifier>();
        }

        protected override void EnsureCanExecute()
        {
            if (this.Identifiers.Count == 0)
                throw new CommandCannotExecuteException("Command cannot execute without any item identifier");
        }

        protected override string BuildXmlCore()
        {
            const string xml = @"<ItemIds>{0}</ItemIds>";

            var builder = new StringBuilder();
            foreach (var identifier in this.Identifiers)
            {
                builder.Append($"<t:ItemId Id='{identifier.Id}'/>");
            }

            return string.Format(xml, builder);
        }
    }
}