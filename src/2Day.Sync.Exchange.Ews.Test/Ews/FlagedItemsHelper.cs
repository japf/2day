using System;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    public class FlagedItemsHelper
    {
        private readonly EwsSyncServer server;
        private readonly EwsSyncService service;
        private EwsFolderIdentifier searchFolderIdentifier;

        public EwsFolderIdentifier SearchFolderIdentifier
        {
            get { return this.searchFolderIdentifier; }
        }

        public FlagedItemsHelper(EwsSyncServer server, EwsSyncService service)
        {
            this.server = server;
            this.service = service;
        }

        public async Task<EwsTask> CreateFlaggedItem(string subject)
        {
            // setup
            var createResult = await this.server.CreateEmailAsync(EwsKnownFolderIdentifiers.Inbox, subject, "world", "me@2day-app.com");
            Assert.IsNotNull(createResult);

            this.searchFolderIdentifier = await this.service.EnsureSearchFolderAsync(this.server);
            Assert.IsNotNull(this.searchFolderIdentifier);

            var ewsTask = new EwsTask
            {
                Type = EwsItemType.Item,

                Id = createResult.Identifiers[0].Id,
                ChangeKey = createResult.Identifiers[0].ChangeKey,
                ParentFolderId = createResult.Identifiers[0].ParentFolderId,
                ParentFolderChangeKey = createResult.Identifiers[0].ParentFolderChangeKey,

                DueDate = DateTime.Now.Date,
                Categories = new[] { "cat1", "cat2 " },
                Changes = EwsFields.DueDate | EwsFields.Categories
            };

            // act
            var updateResult = await this.server.UpdateItemAsync(ewsTask);

            // check
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(1, updateResult.Identifiers.Count);
            Assert.AreEqual(ewsTask.Id, updateResult.Identifiers[0].Id);

            ewsTask.ChangeKey = updateResult.Identifiers[0].ChangeKey;

            return ewsTask;
        }
    }
}