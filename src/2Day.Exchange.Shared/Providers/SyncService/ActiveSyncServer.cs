using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.ActiveSync.Commands;
using Chartreuse.Today.Exchange.ActiveSync.Exceptions;
using Chartreuse.Today.Exchange.Shared.Commands;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Exchange.Providers.SyncService;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    public class ActiveSyncServer
    {
        private const string ProtocolVersion = "14.0";
        private const bool UseSSL = true;
        private const string DeviceType = "WinPhone7";

        private static readonly List<string> AvailableServers = new List<string>()
        {
            "https://bay-m.hotmail.com", 
            "https://dub-m.hotmail.com", 
            "https://m.hotmail.com", 
            "https://snt-m.hotmail.com", 
            "https://blu-m.hotmail.com", 
            "https://col-m.hotmail.com",
            "https://outlook.office365.com"
        };

        private ASRequestSettings settings;

        public uint PolicyKey
        {
            get { return this.settings.PolicyKey; }
        }

        public ActiveSyncServer(string serverName, string login, string password, string deviceId, uint policyKey)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            if (deviceId.Length > 32)
                deviceId = deviceId.Substring(deviceId.Length - 32, 32);

            this.settings = new ASRequestSettings(serverName, login, password, ProtocolVersion, UseSSL, deviceId, DeviceType, policyKey);
        }

        public async Task<string> GetAutoDiscoveredServer()
        {
            foreach (var serverUri in AvailableServers)
            {
                this.settings = new ASRequestSettings(serverUri, this.settings.Login, this.settings.Password, this.settings.ProtocolVersion, this.settings.UseSSL, this.settings.DeviceId, this.settings.DeviceType, this.settings.PolicyKey);
                try
                {
                    FolderSyncCommandResult result = await this.FolderSync("0");
                    if (!string.IsNullOrEmpty(result.SyncKey))
                    {
                        if (!string.IsNullOrEmpty(this.settings.HostName))
                            return this.settings.HostName;

                        return serverUri;
                    }
                }
                catch (CommandException)
                {
                    // do nothing and try the next server
                }
            }

            // didn't find any server to handle current credentials
            return null;
        }

        public async Task<FolderSyncCommandResult> FolderSync(string syncKey)
        {
            if (string.IsNullOrEmpty(syncKey))
                throw new ArgumentNullException(nameof(syncKey));

            FolderSyncCommand command = new FolderSyncCommand(new FolderSyncCommandParameter(syncKey), this.settings);

            ResponseResult<FolderSyncCommandResult> result = await command.Execute();

            if (ActiveSyncErrorHelper.IsStatusProvisioningError(result.Data?.Status, result?.Error))
            {
                await this.TryDoProvisioning();
                result = await command.Execute();
            }

            if (result.Error != null)
            {
                throw result.Error;
            }

            LogService.Log("ActiveSyncServer", "Folder sync success count: " + result.Data.AddedFolders.Count);

            return result.Data;
        }

        public async Task<SyncCommandResult> Sync(string syncKey, string folderId, ExchangeChangeSet changeset)
        {
            if (string.IsNullOrEmpty(syncKey))
                throw new ArgumentNullException(nameof(syncKey));
            if (string.IsNullOrEmpty(folderId))
                throw new ArgumentNullException(nameof(folderId));
            if (changeset == null)
                throw new ArgumentNullException(nameof(changeset));

            LogService.Log("ActiveSyncServer", string.Format("Syncing key: {0} folder id: {1}", syncKey.TakeLast(5), folderId != null ? folderId.TakeLast(5) : "<none>"));

            SyncCommand command = new SyncCommand(new SyncCommandParameter(syncKey, folderId, changeset), this.settings);

            ResponseResult<SyncCommandResult> result = await command.Execute();

            if (ActiveSyncErrorHelper.IsStatusProvisioningError(result.Data?.Status, result?.Error))
            {
                await this.TryDoProvisioning();
                result = await command.Execute();
            }

            if (result.Error != null)
            {
                throw result.Error;
            }

            return result.Data;
        }

        private async Task TryDoProvisioning()
        {
            LogService.Log("ActiveSyncServer", $"Got provisioning error, trying to provision the device");

            // try again with provisioning request + ack
            // send provisioning request
            ProvisionCommand provisionReq = new ProvisionCommand(new ProvisionCommandParameter(), this.settings);
            ResponseResult<ProvisionCommandResult> r1 = await provisionReq.Execute();

            // in the response, we have a temporary policy key we must send back to the server
            ProvisionCommand provisionAck = new ProvisionCommand(new ProvisionCommandParameter(r1.Data.PolicyKey), this.settings);
            ResponseResult<ProvisionCommandResult> r2 = await provisionAck.Execute();

            // int the response of the hack, we have the permanent policy key we will need to use in the future
            uint policyKey = uint.Parse(r2.Data.PolicyKey);

            // update the settings with the new policy key
            // it will be saved in the workbook's settings for future usage in parent method
            this.settings.PolicyKey = policyKey;
        }
    }
}
