using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateFolderParameter : EwsRequestParameterBuilderBase
    {
        public List<string> Names { get; set; }

        public EwsFolderIdentifier ParentFolderIdentifier { get; set; }

        public CreateFolderParameter()
        {
            this.Names = new List<string>();
        }

        protected override void EnsureCanExecute()
        {
            if (this.Names.Count == 0)
                throw new CommandCannotExecuteException("A least one name must be given as parameter");
            if (this.ParentFolderIdentifier == null)
                throw new CommandCannotExecuteException("ParentFolderIdentifier must not be null");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <ParentFolderId>" +
                "      {0}" +
                "   </ParentFolderId>" +
                "   <Folders>" +
                "       {1}" +
                "   </Folders>";

            var builder = new StringBuilder();
            foreach (var name in this.Names)
                builder.AppendLine(string.Format("<t:TasksFolder><t:DisplayName>{0}</t:DisplayName></t:TasksFolder>", name));

            return string.Format(command, this.ParentFolderIdentifier.GetXml(), builder);
        }
    }
}