using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.ExtendedExecution;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.App.Tools
{
    /// <summary>
    /// This class is responsible for handling application suspension event
    /// </summary>
    internal class SuspensionManager
    {
        private readonly IPersistenceLayer persistenceLayer;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly ITileManager tileManager;
        private SuspendingDeferral deferral;
        private bool suspensionPending;

        internal SuspensionManager(IPersistenceLayer persistenceLayer, ISynchronizationManager synchronizationManager, ITileManager tileManager)
        {
            if (persistenceLayer == null)
                throw new ArgumentNullException(nameof(persistenceLayer));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (tileManager == null)
                throw new ArgumentNullException(nameof(tileManager));

            this.persistenceLayer = persistenceLayer;
            this.synchronizationManager = synchronizationManager;
            this.tileManager = tileManager;

            this.synchronizationManager.OperationCompleted += this.OnSyncCompleted;
            this.synchronizationManager.OperationFailed += this.OnSyncFailed;
        }

        private async void OnSyncFailed(object sender, SyncFailedEventArgs e)
        {
            if (this.suspensionPending)
            {
                await this.SaveAsync();
                this.TryCompleteCurrentDeferral();
            }
        }

        private async void OnSyncCompleted(object sender, EventArgs<string> e)
        {
            if (this.suspensionPending)
            {
                await this.SaveAsync();
                this.TryCompleteCurrentDeferral();
            }
        }

        public async void SuspendAsync(SuspendingEventArgs suspendingEventArgs)
        {
            this.suspensionPending = true;
            this.deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();

            var mainPage = TreeHelper.TryGetPageFromRootFrame<MainPage>();
            if (mainPage != null)
                mainPage.OnSuspending();

            bool continueExecution = false;

            // if sync is running, try to continue execution until it completes
            if (this.synchronizationManager.IsSyncRunning)
            {
                using (var session = new ExtendedExecutionSession())
                {
                    session.Reason = ExtendedExecutionReason.SavingData;
                    session.Description = StringResources.SyncProgress_SyncInProgress;

                    var result = await session.RequestExtensionAsync();
                    if (result == ExtendedExecutionResult.Allowed)
                    {
                        continueExecution = true;
                    }
                }
            }

            if (!continueExecution)
            {
                await this.SaveAsync();
                this.tileManager.UpdateTiles();
                this.TryCompleteCurrentDeferral();
            }
        }

        public  async Task SaveAsync()
        {
            this.persistenceLayer.Save();
            await this.synchronizationManager.SaveAsync();
            await LogService.SaveAsync();
        }

        private void TryCompleteCurrentDeferral()
        {
            if (this.deferral != null)
            {
                this.deferral.Complete();
                this.deferral = null;
            }
        }
    }
}