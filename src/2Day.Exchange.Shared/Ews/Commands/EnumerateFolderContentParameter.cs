using System;
using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class EnumerateFolderContentParameter : EwsRequestParameterBuilderBase
    {
        private const int MaxEntriesReturned = 500;

        public EwsFolderIdentifier FolderIdentifier { get; set; }

        protected override void EnsureCanExecute()
        {
            if (string.IsNullOrEmpty("FolderId"))
                throw new CommandCannotExecuteException("Folder is cannot be empty");
        }

        protected override Dictionary<string, string> GetCommandAttributes()
        {
            return new Dictionary<string, string>
            {
                { "Traversal", "Shallow" }
            };
        }

        protected override string BuildXmlCore()
        {
            const string command =
                "  <ItemShape>" +
                "      <t:BaseShape>IdOnly</t:BaseShape>" +
                "      <t:AdditionalProperties>" +
                "          <t:FieldURI FieldURI='item:ParentFolderId' />" +
                "      </t:AdditionalProperties>" +
                "  </ItemShape>" +
                "  <IndexedPageItemView MaxEntriesReturned='{0}' Offset='{1}' BasePoint='Beginning' />" +                
                /*"  <Restriction>" +
                "      <t:Or>" +
                "          <t:Not>" +
                "              <t:Exists>" +
                "                  <t:ExtendedFieldURI PropertySetId='00062003-0000-0000-C000-000000000046' PropertyId='33039' PropertyType='SystemTime' />" +
                "              </t:Exists>" +
                "          </t:Not>" +
                "          <t:IsGreaterThanOrEqualTo>" +
                "              <t:ExtendedFieldURI PropertySetId='00062003-0000-0000-C000-000000000046' PropertyId='33039' PropertyType='SystemTime' />" +
                "              <t:FieldURIOrConstant>" +
                "                  <t:Constant Value='{2}' />" +
                "              </t:FieldURIOrConstant>" +
                "          </t:IsGreaterThanOrEqualTo>" +
                "      </t:Or>" +
                "  </Restriction>" +*/
                "  <ParentFolderIds>" +
                "      {2}" +
                "  </ParentFolderIds>";

            string xmlCommand = string.Format(
                command, 
                MaxEntriesReturned, 
                0, // todo ews: managed sync offset
                // DateTime.Today.AddDays(-7).ToEwsDateTimeValue(DateTimeKind.Local),
                this.FolderIdentifier.GetXml());

            return xmlCommand;
        }
    }
}