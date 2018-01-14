using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteFolderParameter : EwsRequestParameterBuilderBase
    {
        public List<EwsFolderIdentifier> Identifiers { get; set; }

        protected override Dictionary<string, string> GetCommandAttributes()
        {
            return new Dictionary<string, string> { { "DeleteType", "HardDelete" } };
        }

        public DeleteFolderParameter()
        {
            this.Identifiers = new List<EwsFolderIdentifier>();
        }

        protected override void EnsureCanExecute()
        {
            if (this.Identifiers.Count == 0)
                throw new CommandCannotExecuteException("A least one folder identifier must be given as parameter");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <FolderIds>" +
                "      {0}" +
                "   </FolderIds>";

            var builder = new StringBuilder();
            foreach (var identifier in this.Identifiers)
                builder.AppendLine(identifier.GetXml());

            return string.Format(command, builder);
        }
    }
}