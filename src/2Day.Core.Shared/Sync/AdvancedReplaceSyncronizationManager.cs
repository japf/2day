using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public class AdvancedReplaceSyncronizationManager : AdvancedSyncronizationManagerBase
    {
        public AdvancedReplaceSyncronizationManager(IWorkbook workbook, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager) 
            : base(workbook, synchronizationManager, trackingManager)
        {
        }

        protected override async Task<bool> AdvancedSyncCore()
        {
            this.TrackingManager.TagEvent("Sync replace", new Dictionary<string, string>
            {
                { "service", this.SynchronizationManager.ActiveService.ToString() }
            });

            using (this.Workbook.WithTransaction())
            {
                int taskWithoutSyncId = this.Workbook.Tasks.Count(t => string.IsNullOrWhiteSpace(t.SyncId));
                if (taskWithoutSyncId > 0)
                {
                    var messageBoxService = Ioc.Resolve<IMessageBoxService>();
                    var result = await messageBoxService.ShowAsync(
                        StringResources.Dialog_TitleConfirmation,
                        string.Format(StringResources.Message_DeleteNoSyncIdFormat, taskWithoutSyncId),
                        DialogButton.OKCancel);

                    if (result != DialogResult.OK)
                        return false;
                }

                // simple replace mode: 
                //  * clear all content from 2Day
                //  * reset sync manager but keep settings (login, password, etc.)
                //  * sync again
                this.Workbook.RemoveAll();

                var currentService = this.SynchronizationManager.ActiveService;

                this.SynchronizationManager.Reset(clearSettings: false);
                this.SynchronizationManager.ActiveService = currentService;

                await this.SynchronizationManager.Sync();
            }

            return true;
        }
    }
}