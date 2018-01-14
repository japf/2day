using System.Collections.Generic;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateEmailParameter : EwsRequestParameterBuilderBase
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public string Recipient { get; set; }

        public EwsFolderIdentifier ParentFolder { get; private set; }

        public CreateEmailParameter(EwsFolderIdentifier parentFolder)
        {
            this.ParentFolder = parentFolder;
        }

        protected override Dictionary<string, string> GetCommandAttributes()
        {
            return new Dictionary<string, string>
            {
                { "MessageDisposition", "SaveOnly" }
            };
        }


        protected override void EnsureCanExecute()
        {
            if (string.IsNullOrWhiteSpace(this.Subject))
                throw new CommandCannotExecuteException("Subject cannot be null or empty");
            if (string.IsNullOrWhiteSpace(this.Body))
                throw new CommandCannotExecuteException("Subject cannot be null or empty");
            if (string.IsNullOrWhiteSpace(this.Recipient))
                throw new CommandCannotExecuteException("Subject cannot be null or empty");
        }

        protected override string BuildXmlCore()
        {
            const string command = @"
                  <m:SavedItemFolderId>
                    {0}
                  </m:SavedItemFolderId>
                  <m:Items>
                    <t:Message>
                      <t:Subject>{1}</t:Subject>
                      <t:Body BodyType='HTML'>{2}</t:Body>
                      <t:ToRecipients>
                        <t:Mailbox>
                          <t:EmailAddress>{3}</t:EmailAddress>
                        </t:Mailbox>
                      </t:ToRecipients>
                    </t:Message>
                  </m:Items>";

            string xml = string.Format(command, this.ParentFolder.GetXml(), this.Subject, this.Body, this.Recipient);

            return xml;
        }
    }
}