using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Shared;

namespace Chartreuse.Today.Exchange.Ews.AutoDiscover
{
    public class AutoDiscoverResult : IResponseParser
    {
        private const string NsEwsAutodiscover = "ad";
        private const string NsEwsOutlookAutodiscover = "oad";

        public string InternalEwsUrl { get; private set; }

        public string ExternalEwsUrl { get; private set; }

        public string RedirectEmailAddress { get; private set; }

        public void ParseResponse(string commandName, WebRequestResponse response)
        {
            if (response.Response.StatusCode == HttpStatusCode.Unauthorized)
                throw new CommandAuthorizationException(string.Format("Authorization failed: {0}", response.Response.ReasonPhrase));

            string xml = response.ResponseBody;

            string xmlValidationErrorMessage;
            if (!EwsXmlHelper.IsValidXml(xml, out xmlValidationErrorMessage))
                throw new CommandResponseXmlException(string.Format("Invalid xml content: {0}", xmlValidationErrorMessage));

            var root = XDocument.Load(new StringReader(xml)).Root;
            if (response.Response.IsSuccessStatusCode)
            {
                // read error section
                var error = root.XGetChild("Response/Error", NsEwsAutodiscover);
                if (error != null)
                {
                    string errorCode = error.XGetChildValue<string>("ErrorCode", NsEwsAutodiscover);
                    string message = error.XGetChildValue<string>("Message", NsEwsAutodiscover);
                    string debugData = error.XGetChildValue<string>("DebugData", NsEwsAutodiscover);

                    LogService.Log("AutoDiscoverResult", string.Format("Error response code: {0} message: {1} debug data: {2}", errorCode, message, debugData));
                    return;
                }

                // read protocols section
                var nodes = root.XGetChildrenOf("Response/Account", NsEwsOutlookAutodiscover);
                foreach (var node in nodes)
                {
                    if (node.Name != null && node.Name.LocalName.Equals("Protocol", StringComparison.OrdinalIgnoreCase))
                    {
                        var type = node.XGetChildValue<string>("Type", NsEwsOutlookAutodiscover);
                        var ewsUrl = node.XGetChildValue<string>("EwsUrl", NsEwsOutlookAutodiscover);

                        // read more about protocol type: http://blogs.technet.com/b/exchange/archive/2008/09/26/3406344.aspx
                        if (type.Equals("exch", StringComparison.OrdinalIgnoreCase))
                            this.InternalEwsUrl = ewsUrl;
                        else if (type.Equals("expr", StringComparison.OrdinalIgnoreCase))
                            this.ExternalEwsUrl = ewsUrl;
                    } 
                    else if (node.Name != null && node.Name.LocalName.Equals("RedirectAddr", StringComparison.OrdinalIgnoreCase))
                    {
                        this.RedirectEmailAddress = node.Value;
                    }
                }
            }
        }
    }
    /*
    POX response documentation http://msdn.microsoft.com/en-us/library/office/bb204070(v=exchg.150).aspx
    
    <Autodiscover xmlns="http://schemas.microsoft.com/exchange/autodiscover/responseschema/2006">
      <Response xmlns="http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a">
        <User>
          <DisplayName>Jeremy Alles</DisplayName>
          <LegacyDN>/o=ExchangeLabs/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Recipients/cn=46e3383036ec4bcba18974d26bfc1e33-jeremy.alle</LegacyDN>
          <AutoDiscoverSMTPAddress>jeremy.alles@vercors.onmicrosoft.com</AutoDiscoverSMTPAddress>
          <DeploymentId>8e85ca31-da1d-4277-a0f9-c86572a65bde</DeploymentId>
        </User>

        1/ error
        <Error Time="06:48:34.3039282" Id="424529772">
            <ErrorCode>500</ErrorCode>
            <Message>The email address can't be found.</Message>
            <DebugData />
        </Error>
     
        <Account>
              2/ redirect
         <RedirectAddr>bill@home.com</RedirectAddr>
     
          3/ real account
          <AccountType>email</AccountType>
          <Action>settings</Action>
          <MicrosoftOnline>True</MicrosoftOnline>
          <Protocol>
            <Type>EXCH</Type>
            <Server>40469cb6-9658-41b4-99fe-c062fbbc152a@vercors.onmicrosoft.com</Server>
            <ServerDN>/o=ExchangeLabs/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Configuration/cn=Servers/cn=40469cb6-9658-41b4-99fe-c062fbbc152a@vercors.onmicrosoft.com</ServerDN>
            <ServerVersion>73C08419</ServerVersion>
            <MdbDN>/o=ExchangeLabs/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Configuration/cn=Servers/cn=40469cb6-9658-41b4-99fe-c062fbbc152a@vercors.onmicrosoft.com/cn=Microsoft Private MDB</MdbDN>
            <PublicFolderServer>outlook.office365.com</PublicFolderServer>
            <AuthPackage>Anonymous</AuthPackage>
            <ASUrl>https://outlook.office365.com/EWS/Exchange.asmx</ASUrl>
            <EwsUrl>https://outlook.office365.com/EWS/Exchange.asmx</EwsUrl>
            <EmwsUrl>https://outlook.office365.com/EWS/Exchange.asmx</EmwsUrl>
            <SharingUrl>https://outlook.office365.com/EWS/Exchange.asmx</SharingUrl>
            <EcpUrl>https://outlook.office365.com/ecp/</EcpUrl>
            <EcpUrl-um>?rfr=olk&amp;p=customize/voicemail.aspx&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-um>
            <EcpUrl-aggr>?rfr=olk&amp;p=personalsettings/EmailSubscriptions.slab&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-aggr>
            <EcpUrl-mt>PersonalSettings/DeliveryReport.aspx?rfr=olk&amp;exsvurl=1&amp;IsOWA=&lt;IsOWA&gt;&amp;MsgID=&lt;MsgID&gt;&amp;Mbx=&lt;Mbx&gt;&amp;realm=vercors.onmicrosoft.com</EcpUrl-mt>
            <EcpUrl-ret>?rfr=olk&amp;p=organize/retentionpolicytags.slab&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-ret>
            <EcpUrl-sms>?rfr=olk&amp;p=sms/textmessaging.slab&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-sms>
            <EcpUrl-publish>customize/calendarpublishing.slab?rfr=olk&amp;exsvurl=1&amp;FldID=&lt;FldID&gt;&amp;realm=vercors.onmicrosoft.com</EcpUrl-publish>
            <EcpUrl-photo>PersonalSettings/EditAccount.aspx?rfr=olk&amp;chgPhoto=1&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-photo>
            <EcpUrl-connect>Connect/Main.aspx?rfr=olk&amp;exsvurl=1&amp;Provider=&lt;Provider&gt;&amp;Action=&lt;Action&gt;&amp;realm=vercors.onmicrosoft.com</EcpUrl-connect>
            <EcpUrl-tm>?rfr=olk&amp;ftr=TeamMailbox&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-tm>
            <EcpUrl-tmCreating>?rfr=olk&amp;ftr=TeamMailboxCreating&amp;SPUrl=&lt;SPUrl&gt;&amp;Title=&lt;Title&gt;&amp;SPTMAppUrl=&lt;SPTMAppUrl&gt;&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-tmCreating>
            <EcpUrl-tmEditing>?rfr=olk&amp;ftr=TeamMailboxEditing&amp;Id=&lt;Id&gt;&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-tmEditing>
            <EcpUrl-extinstall>Extension/InstalledExtensions.slab?rfr=olk&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-extinstall>
            <OOFUrl>https://outlook.office365.com/EWS/Exchange.asmx</OOFUrl>
            <UMUrl>https://outlook.office365.com/EWS/UM2007Legacy.asmx</UMUrl>
            <OABUrl>https://outlook.office365.com/OAB/4cbbeddb-c9ac-40f2-9548-dc587223be5f/</OABUrl>
            <ServerExclusiveConnect>off</ServerExclusiveConnect>
          </Protocol>
          <Protocol>
            <Type>EXPR</Type>
            <Server>outlook.office365.com</Server>
            <SSL>On</SSL>
            <AuthPackage>Basic</AuthPackage>
            <ASUrl>https://outlook.office365.com/EWS/Exchange.asmx</ASUrl>
            <EwsUrl>https://outlook.office365.com/EWS/Exchange.asmx</EwsUrl>
            <EmwsUrl>https://outlook.office365.com/EWS/Exchange.asmx</EmwsUrl>
            <SharingUrl>https://outlook.office365.com/EWS/Exchange.asmx</SharingUrl>
            <EcpUrl>https://outlook.office365.com/ecp/</EcpUrl>
            <EcpUrl-um>?rfr=olk&amp;p=customize/voicemail.aspx&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-um>
            <EcpUrl-aggr>?rfr=olk&amp;p=personalsettings/EmailSubscriptions.slab&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-aggr>
            <EcpUrl-mt>PersonalSettings/DeliveryReport.aspx?rfr=olk&amp;exsvurl=1&amp;IsOWA=&lt;IsOWA&gt;&amp;MsgID=&lt;MsgID&gt;&amp;Mbx=&lt;Mbx&gt;&amp;realm=vercors.onmicrosoft.com</EcpUrl-mt>
            <EcpUrl-ret>?rfr=olk&amp;p=organize/retentionpolicytags.slab&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-ret>
            <EcpUrl-sms>?rfr=olk&amp;p=sms/textmessaging.slab&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-sms>
            <EcpUrl-publish>customize/calendarpublishing.slab?rfr=olk&amp;exsvurl=1&amp;FldID=&lt;FldID&gt;&amp;realm=vercors.onmicrosoft.com</EcpUrl-publish>
            <EcpUrl-photo>PersonalSettings/EditAccount.aspx?rfr=olk&amp;chgPhoto=1&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-photo>
            <EcpUrl-connect>Connect/Main.aspx?rfr=olk&amp;exsvurl=1&amp;Provider=&lt;Provider&gt;&amp;Action=&lt;Action&gt;&amp;realm=vercors.onmicrosoft.com</EcpUrl-connect>
            <EcpUrl-tm>?rfr=olk&amp;ftr=TeamMailbox&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-tm>
            <EcpUrl-tmCreating>?rfr=olk&amp;ftr=TeamMailboxCreating&amp;SPUrl=&lt;SPUrl&gt;&amp;Title=&lt;Title&gt;&amp;SPTMAppUrl=&lt;SPTMAppUrl&gt;&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-tmCreating>
            <EcpUrl-tmEditing>?rfr=olk&amp;ftr=TeamMailboxEditing&amp;Id=&lt;Id&gt;&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-tmEditing>
            <EcpUrl-extinstall>Extension/InstalledExtensions.slab?rfr=olk&amp;exsvurl=1&amp;realm=vercors.onmicrosoft.com</EcpUrl-extinstall>
            <OOFUrl>https://outlook.office365.com/EWS/Exchange.asmx</OOFUrl>
            <UMUrl>https://outlook.office365.com/EWS/UM2007Legacy.asmx</UMUrl>
            <OABUrl>https://outlook.office365.com/OAB/4cbbeddb-c9ac-40f2-9548-dc587223be5f/</OABUrl>
            <ServerExclusiveConnect>on</ServerExclusiveConnect>
            <CertPrincipalName>msstd:outlook.com</CertPrincipalName>
            <EwsPartnerUrl>https://outlook.office365.com/EWS/Exchange.asmx</EwsPartnerUrl>
            <GroupingInformation>DB4PR01</GroupingInformation>
          </Protocol>
          <Protocol>
            <Type>WEB</Type>
            <Internal>
              <OWAUrl AuthenticationMethod="LiveIdFba, OAuth">https://outlook.office365.com/owa/</OWAUrl>
              <Protocol>
                <Type>EXCH</Type>
                <ASUrl>https://outlook.office365.com/EWS/Exchange.asmx</ASUrl>
              </Protocol>
            </Internal>
            <External>
              <OWAUrl AuthenticationMethod="Fba">https://outlook.office365.com/owa/vercors.onmicrosoft.com/</OWAUrl>
              <Protocol>
                <Type>EXPR</Type>
                <ASUrl>https://outlook.office365.com/EWS/Exchange.asmx</ASUrl>
              </Protocol>
            </External>
          </Protocol>
        </Account>
      </Response>
    </Autodiscover>
     */
}