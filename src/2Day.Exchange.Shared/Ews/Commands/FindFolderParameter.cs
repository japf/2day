using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class FindFolderParameter : EwsRequestParameterBuilderBase
    {
        public EwsFolderIdentifier ParentFolderIdentifier { get; set; }

        protected override Dictionary<string, string> GetCommandAttributes()
        {
            return new Dictionary<string, string>
            {
                { "Traversal", "Deep" }
            };
        }

        protected override void EnsureCanExecute()
        {
            if (this.ParentFolderIdentifier == null)
                throw new CommandCannotExecuteException("ParentFolderIdentifier must not be null");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <FolderShape>" +
                "       <t:BaseShape>Default</t:BaseShape>" +
                "   </FolderShape>" +
                "   <ParentFolderIds>" +
                "      {0}" +
                "   </ParentFolderIds>";

            return string.Format(command, this.ParentFolderIdentifier.GetXml());
        }
    }
}