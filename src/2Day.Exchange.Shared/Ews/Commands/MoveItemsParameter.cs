using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class MoveItemsParameter : EwsRequestParameterBuilderBase
    {
        public List<EwsItemIdentifier> Identifiers { get; private set; }

        public EwsFolderIdentifier Target { get; set; }

        public MoveItemsParameter()
        {
            this.Identifiers = new List<EwsItemIdentifier>();
        }

        protected override void EnsureCanExecute()
        {
            if (this.Identifiers.Count == 0)
                throw new CommandCannotExecuteException("Command cannot execute without any item identifier");
            if (this.Target == null)
                throw new CommandCannotExecuteException("Target must be not null");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <m:ToFolderId>" +
                "      {0}" +
                "   </m:ToFolderId>" +
                "   <m:ItemIds>" +
                "      {1}" +
                "   </m:ItemIds>";

            const string itemIdTemplate = "<t:ItemId Id='{0}' ChangeKey='{1}'></t:ItemId>";

            var builder = new StringBuilder();
            foreach (var identifier in this.Identifiers)
            {
                builder.AppendLine(string.Format(itemIdTemplate, identifier.Id, identifier.ChangeKey));
            }

            var content = string.Format(
                command,
                this.Target.GetXml(),
                builder.ToString());

            return content;
        }
    }
}