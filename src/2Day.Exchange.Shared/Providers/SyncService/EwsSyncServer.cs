using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Ews.Commands;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Providers.SyncService
{
    public class EwsSyncServer
    {
        private readonly EwsRequestSettings settings;
        private static GetFolderIdentifiersResult cachedFolderIdentifiersResult;

        public EwsSyncServer(EwsRequestSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.settings = settings;
        }

        public async Task<GetFolderIdentifiersResult> GetRootFolderIdentifiersAsync(bool useCache = true)
        {
#if DEBUG
            if (useCache && cachedFolderIdentifiersResult != null)
                return cachedFolderIdentifiersResult;
#endif
            var command = new GetFolderIdentifiersCommand(new GetFolderIdentifiersParameter(), this.settings);

            var result = await command.Execute();

            if (result.Error != null)
                throw result.Error;

            cachedFolderIdentifiersResult = result.Data;

            return result.Data;
        }

        public async Task<FindFolderResult> GetSubFoldersAsync(EwsFolderIdentifier identifier)
        {
            var parameter = new FindFolderParameter {ParentFolderIdentifier = identifier};

            var command = new FindFolderCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<CreateFolderResult> CreateFoldersAsync(IEnumerable<string> names, EwsFolderIdentifier identifier)
        {
            var parameter = new CreateFolderParameter {ParentFolderIdentifier = identifier};
            parameter.Names.AddRange(names);

            var command = new CreateFolderCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<DeleteFolderResult> DeleteFoldersAsync(IEnumerable<EwsFolderIdentifier> identifiers)
        {
            var parameter = new DeleteFolderParameter();
            parameter.Identifiers.AddRange(identifiers);

            var command = new DeleteFolderCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }        

        public async Task<List<EwsItemIdentifier>> EnumerateFolderContentAsync(EwsFolderIdentifier identifier, EwsItemType itemType = EwsItemType.Task)
        {
            var parameter = new EnumerateFolderContentParameter { FolderIdentifier = identifier };

            var command = new EnumerateFolderContentCommand(parameter, this.settings);

            var result = await command.Execute();
            var identifiers = result.Data.ItemIdentifiers;

            if (itemType == EwsItemType.Item)
            {
                // remove from the returned item identifiers those that are in the 'Deleted Items' folders
                if (identifiers != null && cachedFolderIdentifiersResult != null && cachedFolderIdentifiersResult.DeletedItemsFolderIdentifier != null && identifier != cachedFolderIdentifiersResult.DeletedItemsFolderIdentifier)
                {
                    int countBefore = identifiers.Count;
                    identifiers = identifiers.Where(id => id.ParentFolderId != cachedFolderIdentifiersResult.DeletedItemsFolderIdentifier.Id).ToList();
                    if (identifiers.Count != countBefore)
                        LogService.Log("EwsSyncServer", $"Removed {countBefore - identifiers.Count} identifiers that are in the 'Delete Folder' folder");
                }
            }

            return identifiers;
        }

        public async Task<List<EwsTask>> DownloadFolderContentAsync(IEnumerable<EwsItemIdentifier> identifiers, EwsItemType ewsItemType)
        {
            if (!identifiers.Any())
                return new List<EwsTask>();

            var parameter = new GetItemParameter(ewsItemType);
            foreach (var identifier in identifiers)
                parameter.ItemIdentifiers.Add(identifier);

            var command = new GetItemCommand(parameter, this.settings);

            var result = await command.Execute();

            // set item type
            foreach (var ewsTask in result.Data.Tasks)
                ewsTask.Type = ewsItemType;

            return result.Data.Tasks;
        }

        public async Task<CreateItemsResult> CreateItemAsync(EwsTask task, EwsFolderIdentifier parentFolder = null)
        {
            return await this.CreateItemAsync(new[] {task}, parentFolder);
        }

        public async Task<CreateItemsResult> CreateItemAsync(IEnumerable<EwsTask> tasks, EwsFolderIdentifier parentFolder = null)
        {
            if (parentFolder == null)
                parentFolder = EwsKnownFolderIdentifiers.Tasks;

            var command = new CreateItemsCommand(new CreateItemsParameter(tasks, parentFolder), this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<MoveItemsResult> DeleteItemsAsync(IEnumerable<EwsItemIdentifier> identifiers)
        {
            var parameter = new DeleteItemsParameter();
            parameter.Identifiers.AddRange(identifiers);

            var command = new DeleteItemsCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<DeleteHardItemsResult> HardDeleteItemsAsync(IEnumerable<EwsItemIdentifier> identifiers)
        {
            var parameter = new DeleteHardItemsParameter();
            parameter.Identifiers.AddRange(identifiers);

            var command = new DeleteHardItemsCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<UpdateItemsResult> UpdateItemAsync(EwsTask task)
        {
            return await this.UpdateItemsAsync(new[] { task });
        }

        public async Task<UpdateItemsResult> UpdateItemsAsync(IEnumerable<EwsTask> tasks)
        {
            var parameter = new UpdateItemsParameter();
            foreach (var task in tasks)
                parameter.Tasks.Add(task);

            var command = new UpdateItemsCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<EwsTask> GetTask(string subject, EwsFolderIdentifier folderIdentifier)
        {
            var itemIdentifiers = await this.EnumerateFolderContentAsync(folderIdentifier, EwsItemType.Task);
            var tasks = await this.DownloadFolderContentAsync(itemIdentifiers, EwsItemType.Task);
            var task = tasks.FirstOrDefault(t => t.Subject == subject);

            return task;
        }

        public async Task<CreateFolderResult> CreateSearchFolderAsync(string name)
        {
            var parameter = new CreateSearchFolderParameter {Name = name};

            var command = new CreateSearchFolderCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }

        public async Task<CreateEmailResult> CreateEmailAsync(EwsFolderIdentifier folderIdentifier, string subject, string body, string recipient)
        {
            var parameter = new CreateEmailParameter(folderIdentifier) { Subject = subject, Body = body, Recipient = recipient };

            var command = new CreateEmailCommand(parameter, this.settings);

            var result = await command.Execute();

            return result.Data;
        }
    }
}
