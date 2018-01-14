using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateSearchFolderParameter : EwsRequestParameterBuilderBase
    {
        public string Name { get; set; }

        protected override void EnsureCanExecute()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
                throw new CommandCannotExecuteException("Name must not be defined");
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "   <ParentFolderId>" +
                "      {0}" +
                "   </ParentFolderId>" +
                "   <Folders>" +
                "       <t:SearchFolder>" +
                "           <t:DisplayName>{1}</t:DisplayName>" +
                "           <t:SearchParameters Traversal='Deep'>" +
                "               <t:Restriction>" +
                "                   <t:And>" +
                "                       <t:IsEqualTo>" +
                "                           <t:ExtendedFieldURI PropertyTag='4240' PropertyType='Integer' />" +
                "                           <t:FieldURIOrConstant>" +
                "                               <t:Constant Value='2' />" +
                "                           </t:FieldURIOrConstant>" +
                "                       </t:IsEqualTo>" +
                "                       <t:Excludes>" +
                "                           <t:ExtendedFieldURI PropertyTag='3627' PropertyType='Integer' />" +
                "                           <t:Bitmask Value='8' />" +
                "                       </t:Excludes>" +
                "                       <t:Not>" +
                "                           <t:Contains ContainmentMode='Prefixed' ContainmentComparison='IgnoreCase'>" +
                "                               <t:FieldURI FieldURI='item:ItemClass' />" +
                "                               <t:Constant Value='ipm.task' />" +
                "                           </t:Contains>" +
                "                       </t:Not>" +
                "                       <t:Not>" +
                "                           <t:Exists>" +
                "                               <t:ExtendedFieldURI PropertySetId='00062003-0000-0000-C000-000000000046' PropertyId='33039' PropertyType='SystemTime' />" +
                "                           </t:Exists>" +
                "                       </t:Not>" +
                "                   </t:And>" +
                "               </t:Restriction>" +
                "               <t:BaseFolderIds>" +
                "                   <t:DistinguishedFolderId Id='msgfolderroot' />" +
                "               </t:BaseFolderIds>" +
                "           </t:SearchParameters>" +
                "       </t:SearchFolder>" +
                "   </Folders>";

            string xml = string.Format(command, EwsKnownFolderIdentifiers.SearchFolders.GetXml(), this.Name);

            return xml;
        }
    }
}