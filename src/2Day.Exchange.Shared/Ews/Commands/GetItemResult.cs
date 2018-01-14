using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Ews.Schema;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class GetItemResult : EwsResponseParserBase
    {
        public List<EwsTask> Tasks { get; private set; }

        public GetItemResult()
        {
            this.Tasks = new List<EwsTask>();
        }

        protected override void ParseResponseCore(List<EwsResponseMessage> responseMessages)
        {
            foreach (var responseMessage in responseMessages.Select(m => m.Content))
            {
                foreach (var item in responseMessage.XGetChildrenOf("Items", "m"))
                {
                    var taskBuilder = new EwsTaskBuilder();
                    foreach (var extendedProperty in item.XGetAllChildren("ExtendedProperty"))
                    {
                        int propertyId = extendedProperty.XGetChildNodeAttributeValue<int>("ExtendedFieldURI", "PropertyId");
                        string value = extendedProperty.XGetChildValue<string>("Value");

                        taskBuilder.LoadExtendedProperty(propertyId, value);
                    }

                    var task = taskBuilder.BuildTask();

                    task.Id = item.XGetChildNodeAttributeValue<string>("ItemId", "Id");
                    task.ChangeKey = item.XGetChildNodeAttributeValue<string>("ItemId", "ChangeKey");
                    task.ParentFolderId = item.XGetChildNodeAttributeValue<string>("ParentFolderId", "Id");
                    task.ParentFolderChangeKey = item.XGetChildNodeAttributeValue<string>("ParentFolderId", "ChangeKey");

                    task.Subject = item.XGetChildValue<string>("Subject");

                    task.Body = item.XGetChildValue<string>("Body", "t");
                    task.BodyType = item.XGetChildNodeAttributeValue<EwsBodyType>("Body", "BodyType", "t");

                    task.ReminderIsSet = item.XGetChildValue<bool>("ReminderIsSet", true);
                    if (task.ReminderIsSet)
                    {
                        task.ReminderDate = item.XGetChildValue<DateTime>("ReminderDueBy", true);
                        if (task.ReminderDate.HasValue && task.ReminderDate != DateTime.MinValue)
                        {
                            var reminder = task.ReminderDate.Value;
                            // convert to local time and the value we have in the XML is UTC
                            task.ReminderDate = new DateTime(reminder.Year, reminder.Month, reminder.Day, reminder.Hour, reminder.Minute, reminder.Second, DateTimeKind.Utc).ToLocalTime();                            
                        }
                    }

                    string recurrence = item.XGetChildInnerValue("Recurrence", true);
                    if (!string.IsNullOrWhiteSpace(recurrence))
                        task.ParseRecurrence(recurrence);
                    
                    task.Importance = item.XGetChildValue<EwsImportance>("Importance");

                    var categories = item.XGetChildrenOf("Categories", "t");
                    task.Categories = categories.Select(x => x.Value).ToArray();

                    this.Tasks.Add(task);
                }
            }
        }
    }

    /*
            <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
              <s:Header>
                <ServerVersionInfo MajorVersion="15" MinorVersion="0" MajorBuildNumber="1039" MinorBuildNumber="11" xmlns:h="http://schemas.microsoft.com/exchange/services/2006/types" xmlns="http://schemas.microsoft.com/exchange/services/2006/types" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
              </s:Header>
              <s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                <m:GetItemResponse xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages" xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types">
                  <m:ResponseMessages>
                    <m:GetItemResponseMessage ResponseClass="Success">
                      <m:ResponseCode>NoError</m:ResponseCode>
                      <m:Items>
                        <t:Task>
                          <t:ItemId Id="AAAkAGplcmVteS5hbGxlc0B2ZXJjb3JzLm9ubWljcm9zb2Z0LmNvbQBGAAAAAABjivCyBkfSRJfnH5QjLXveBwCdlcnfVxEMSoRfenadZfEJAAAAAAESAACdlcnfVxEMSoRfenadZfEJAAAT53vSAAA=" ChangeKey="EwAAABYAAACdlcnfVxEMSoRfenadZfEJAAAT51JN" />
                          <t:ParentFolderId Id="AQAkAGplcmVteS5hbGwAZXNAdmVyY29ycy5vbm1pY3Jvc29mdC5jb20ALgAAA2OK8LIGR9JEl+cflCMte94BAJ2Vyd9XEQxKhF96dp1l8QkAAAIBEgAAAA==" ChangeKey="AQAAAA==" />
                          <t:ItemClass>IPM.Task</t:ItemClass>
                          <t:Subject>test</t:Subject>
                          <t:Body BodyType="HTML">body...
                          <t:Categories>
                            <t:String>category1</t:String>
                            <t:String>category2</t:String>
                          </t:Categories>
                          <t:Importance>Normal</t:Importance>
                          <t:ReminderIsSet>false</t:ReminderIsSet>
                          <t:ExtendedProperty>
                            <t:ExtendedFieldURI DistinguishedPropertySetId="Common" PropertyId="34208" PropertyType="SystemTime" />
                            <t:Value>2014-09-23T19:56:31Z</t:Value>
                          </t:ExtendedProperty>
                          <t:ExtendedProperty>
                            <t:ExtendedFieldURI DistinguishedPropertySetId="Common" PropertyId="34208" PropertyType="SystemTime" />
                            <t:Value>2014-09-23T19:56:31Z</t:Value>
                          </t:ExtendedProperty>
                          ...
                        </t:Task>
                      </m:Items>
                    </m:GetItemResponseMessage>
                  </m:ResponseMessages>
                </m:GetItemResponse>
              </s:Body>
            </s:Envelope> 
        */
}