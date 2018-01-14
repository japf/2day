using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Shared;

namespace Chartreuse.Today.Exchange.Ews
{
    public abstract class EwsResponseParserBase : IResponseParser
    {
        private const string ResponseCodeNoError = "NoError";

        public static EventHandler<EventArgs<string>> OnInvalidResponse;

        public void ParseResponse(string commandName, WebRequestResponse response)
        {
            if (response.Response.StatusCode == HttpStatusCode.Unauthorized)
                throw new CommandAuthorizationException(string.Format("Authorization failed: {0}", response.Response.ReasonPhrase));

            if (response.Response.IsSuccessStatusCode)
            {
                string xml = response.ResponseBody;

                string xmlValidationErrorMessage;
                if (!EwsXmlHelper.IsValidXml(xml, out xmlValidationErrorMessage))
                    throw new CommandResponseXmlException(string.Format("Invalid xml content: {0}", xmlValidationErrorMessage));

                XDocument xdoc = XDocument.Load(new StringReader(xml));

                var responseMessages = xdoc.Root.XGetChildrenOf(string.Format("Body/{0}Response/ResponseMessages", commandName));
                var ewsResponseMessages = new List<EwsResponseMessage>();

                foreach (var responseMessage in responseMessages)
                {
                    var ewsResponseMessage = new EwsResponseMessage
                    {
                        Class = responseMessage.XGetAttributeValue<EwsResponseClass>("ResponseClass"),
                        Code = responseMessage.XGetChildValue<string>("ResponseCode", true),
                        Message = responseMessage.XGetChildValue<string>("MessageText", true),
                        Content = responseMessage
                    };

                    if (ewsResponseMessage.Class != EwsResponseClass.Success || ewsResponseMessage.Code != ResponseCodeNoError)
                    {
                        string message = string.Format("response is invalid (class: {0} code: {1} message: {2}) content: {3})", ewsResponseMessage.Class, ewsResponseMessage.Code, ewsResponseMessage.Message, responseMessage);
                        Log(commandName, message);

                        if (OnInvalidResponse != null)
                            OnInvalidResponse(this, new EventArgs<string>(message));
                    }

                    ewsResponseMessages.Add(ewsResponseMessage);
                }

                try
                {
                    this.ParseResponseCore(ewsResponseMessages);
                }
                catch (Exception ex)
                {
                    throw new CommandResponseException(string.Format("Failed to parse {0} response", commandName), ex);
                }
            }
            else
            {
                Log(commandName, string.Format("response does not have HTTP sucess status code (code: {0})", response.Response.StatusCode));
                EwsFault fault = null;
                string xml;
                try
                {
                    xml = response.ResponseBody;
                    XDocument xdoc = XDocument.Load(new StringReader(xml));

                    fault = this.ExtractFault(xdoc);
                    Log(commandName, string.Format("fault: " + fault));
                }
                catch (Exception)
                {
                    response.Response.Content.ReadAsStringAsync().ContinueWith(r => Log(commandName, string.Format("response: " + r.Result)));
                }

                throw new CommandResponseException(string.Format("Failed to read {0} response, fault: {1}", commandName, fault != null ? fault.ToString() : "unknown fault"));
            }
        }

        private static void Log(string command, string message)
        {
            LogService.Log("EWS " + command, message);
        }

        private EwsFault ExtractFault(XDocument xdoc)
        {
            var fault = new EwsFault();
            var elements = xdoc.Root.XGetChildrenOf("Body");

            if (elements.Count > 0)
            {
                fault.FaultCode = elements[0].XGetChildValue<string>("faultcode");
                fault.FaultString = elements[0].XGetChildValue<string>("faultstring");
            }

            var detail = elements[0].XGetChildrenOf("detail", "");
            if (detail.Count > 2)
            {
                fault.ResponseCode = detail[0].Value;
                fault.Message = detail[1].Value;
                fault.LineNumber = detail[2].XGetChildValue<int>("LineNumber");
                fault.LinePosition = detail[2].XGetChildValue<int>("LinePosition");
                fault.Violation = detail[2].XGetChildValue<string>("Violation");    
            }

            return fault;
        }

        protected abstract void ParseResponseCore(List<EwsResponseMessage> responseMessages);
    }
}
/*
 <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <s:Fault>
      <faultcode xmlns:a="http://schemas.microsoft.com/exchange/services/2006/types">a:ErrorSchemaValidation</faultcode>
      <faultstring xml:lang="en-US">The request failed schema validation: The element 'Task' in namespace 'http://schemas.microsoft.com/exchange/services/2006/types' has invalid child element 'T' in namespace 'http://schemas.microsoft.com/exchange/services/2006/types'. List of possible elements expected: 'MimeContent, ItemId, ParentFolderId, ItemClass, Subject, Sensitivity, Body, Attachments, DateTimeReceived, Size, Categories, Importance, InReplyTo, IsSubmitted, IsDraft, IsFromMe, IsResend, IsUnmodified, InternetMessageHeaders, DateTimeSent, DateTimeCreated, ResponseObjects, ReminderDueBy, ReminderIsSet, ReminderMinutesBeforeStart, DisplayCc, DisplayTo, HasAttachments, ExtendedProperty, Culture, ActualWork, AssignedTime, BillingInformation, ChangeCount, Companies, CompleteDate, Contacts, DelegationState, Delegator, DueDate, IsAssignmentEditable, IsComplete, IsRecurring, IsTeamTask, Mileage, Owner, PercentComplete, Recurrence, StartDate, Status, StatusDescription, TotalWork' in namespace 'http://schemas.microsoft.com/exchange/services/2006/types'.</faultstring>
      <detail>
        <e:ResponseCode xmlns:e="http://schemas.microsoft.com/exchange/services/2006/errors">ErrorSchemaValidation</e:ResponseCode>
        <e:Message xmlns:e="http://schemas.microsoft.com/exchange/services/2006/errors">The request failed schema validation.</e:Message>
        <t:MessageXml xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
          <t:LineNumber>1</t:LineNumber>
          <t:LinePosition>518</t:LinePosition>
          <t:Violation>The element 'Task' in namespace 'http://schemas.microsoft.com/exchange/services/2006/types' has invalid child element 'T' in namespace 'http://schemas.microsoft.com/exchange/services/2006/types'. List of possible elements expected: 'MimeContent, ItemId, ParentFolderId, ItemClass, Subject, Sensitivity, Body, Attachments, DateTimeReceived, Size, Categories, Importance, InReplyTo, IsSubmitted, IsDraft, IsFromMe, IsResend, IsUnmodified, InternetMessageHeaders, DateTimeSent, DateTimeCreated, ResponseObjects, ReminderDueBy, ReminderIsSet, ReminderMinutesBeforeStart, DisplayCc, DisplayTo, HasAttachments, ExtendedProperty, Culture, ActualWork, AssignedTime, BillingInformation, ChangeCount, Companies, CompleteDate, Contacts, DelegationState, Delegator, DueDate, IsAssignmentEditable, IsComplete, IsRecurring, IsTeamTask, Mileage, Owner, PercentComplete, Recurrence, StartDate, Status, StatusDescription, TotalWork' in namespace 'http://schemas.microsoft.com/exchange/services/2006/types'.</t:Violation>
        </t:MessageXml>
      </detail>
    </s:Fault>
  </s:Body>
</s:Envelope>

*/