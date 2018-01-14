using System;
using System.Text;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    internal class ProvisionCommandParameter : IRequestParameterBuilder
    {
        private readonly string policyKey;

        public ProvisionCommandParameter(string policyKey = null)
        {
            this.policyKey = policyKey;
        }

        public string BuildXml(string command)
        {
            StringBuilder xmlBuilder = new StringBuilder();

            xmlBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlBuilder.Append("<Provision xmlns=\"Provision\" xmlns:settings=\"Settings\">");
            /*xmlBuilder.Append("    <settings:DeviceInformation>");
            xmlBuilder.Append("        <settings:Set>");
            xmlBuilder.Append("            <settings:Model>Test 1.0</settings:Model>");
            xmlBuilder.Append("            <settings:IMEI>012345678901234</settings:IMEI>");
            xmlBuilder.Append("            <settings:FriendlyName>My Test App</settings:FriendlyName>");
            xmlBuilder.Append("            <settings:OS>Test OS 1.0</settings:OS>");
            xmlBuilder.Append("            <settings:OSLanguage>English</settings:OSLanguage>");
            xmlBuilder.Append("            <settings:PhoneNumber>555-123-4567</settings:PhoneNumber>");
            xmlBuilder.Append("            <settings:MobileOperator>My Phone Company</settings:MobileOperator>");
            xmlBuilder.Append("            <settings:UserAgent>TestAgent</settings:UserAgent>");
            xmlBuilder.Append("        </settings:Set>");
            xmlBuilder.Append("    </settings:DeviceInformation>");*/
            xmlBuilder.Append("     <Policies>");
            xmlBuilder.Append("          <Policy>");
            xmlBuilder.Append("               <PolicyType>MS-EAS-Provisioning-WBXML</PolicyType> ");

            if (!string.IsNullOrEmpty(this.policyKey))
            {
                xmlBuilder.Append(string.Format("               <PolicyKey>{0}</PolicyKey> ", this.policyKey));
                xmlBuilder.Append(string.Format("               <Status>{0}</Status> ", 1));
            }

            xmlBuilder.Append("          </Policy>");
            xmlBuilder.Append("     </Policies>");
            xmlBuilder.Append("</Provision>");

            return string.Format(xmlBuilder.ToString());
        }
    }
}