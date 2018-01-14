using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Net;

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    internal class ProvisionCommandResult : ASResponseParserBase
    {
        public string Status { get; set; }

        public string PolicyKey { get; set; }

        protected override void ParseResponseCore(string commandName, XDocument document, WebRequestResponse response)
        {
            this.Status = document.Element("Provision").Element("Policies").Element("Policy").Element("Status").Value;
            this.PolicyKey = document.Element("Provision").Element("Policies").Element("Policy").Element("PolicyKey").Value;
        }
    }

    /*
     * Sample XML response
        <?xml version="1.0" encoding="utf-8"?>
        <Provision xmlns="Provision:" xmlns:settings="Settings:">
             <settings:DeviceInformation>
                  <settings:Status>1</settings:Status>
             </settings:DeviceInformation>
             <Policies>
                  <Policy>
                       <PolicyType>MS-EAS-Provisioning-WBXML</PolicyType>
                       <Status>1</Status>
                       <PolicyKey>1307199584</PolicyKey>
                       <Data>
                            <EASProvisionDoc>
                                 <DevicePasswordEnabled>1</DevicePasswordEnabled>
                                 <AlphanumericDevicePasswordRequired>1</AlphanumericDevicePasswordRequired> 
                                 <PasswordRecoveryEnabled>1</PasswordRecoveryEnabled>
                                 <RequireStorageCardEncryption>1</RequireStorageCardEncryption>
                                 <AttachmentsEnabled>1</AttachmentsEnabled>
                                 <MinDevicePasswordLength/>
                                 <MaxInactivityTimeDeviceLock>333</MaxInactivityTimeDeviceLock>
                                 <MaxDevicePasswordFailedAttempts>8</MaxDevicePasswordFailedAttempts>
                                 <MaxAttachmentSize/> 
                                 <AllowSimpleDevicePassword>0</AllowSimpleDevicePassword> 
                                 <DevicePasswordExpiration/>
                                 <DevicePasswordHistory>0</DevicePasswordHistory>
                            </EASProvisionDoc>
                       </Data>
                  </Policy>
             </Policies>
        </Provision>
     */
}