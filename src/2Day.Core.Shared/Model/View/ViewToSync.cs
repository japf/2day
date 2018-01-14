using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewToSync : SystemDefinedView
    {
        private static ISynchronizationManager syncManager;

        public override string EmptyHeader
        {
            get { return StringResources.SystemView_ToSync_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_ToSync_EmptyHint; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public ViewToSync(IWorkbook workbook, ISystemView view) : base(workbook, view)
        {
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {            
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => {
                if (syncManager == null && Ioc.HasType<ISynchronizationManager>())
                {
                    syncManager = Ioc.Resolve<ISynchronizationManager>();
                    syncManager.OperationCompleted += (s, e) => this.Tasks.ToList().ForEach(t => this.OnTaskPropertyChanged(t, new PropertyChangedEventArgs(string.Empty)));
                }

                return 
                    string.IsNullOrWhiteSpace(task.SyncId) || 
                    (syncManager != null && syncManager.IsSyncConfigured && syncManager.Metadata != null && syncManager.Metadata.EditedTasks.ContainsKey(task.Id));
            };
        }
    }
}