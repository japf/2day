using System;
using System.Diagnostics;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model.View;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    [DebuggerDisplay("Name: {Name} Rules: {Rules}")]
    public class SmartView : ViewBase, ISmartView
    {
        private readonly ISmartView smartviewOwner;
        private SmartViewHandler handler;
        private string rules;

        public override string EmptyHeader
        {
            get { return StringResources.SmartView_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SmartView_EmptyHint; }
        }

        public new int IconId
        {
            get { return FontIconHelper.IconIdMagic; }
        }

        public string SyncId
        {
            get { return this.smartviewOwner.SyncId; }
            set
            {
                if (this.smartviewOwner.SyncId != value)
                {
                    this.smartviewOwner.SyncId = value;
                    this.RaisePropertyChanged("SyncId");
                }
            }
        }

        public string Rules
        {
            get
            {
                return this.rules;
            }
            set
            {
                if (this.rules != value)
                {
                    this.rules = value;

                    this.handler = SmartViewHandler.FromString(this.rules);
                    this.smartviewOwner.Rules = this.rules;

                    this.Rebuild();

                    this.RaisePropertyChanged("Rules");
                }
            }
        }

        public ISmartView Owner
        {
            get { return this.smartviewOwner; }
        }

        public bool ShowCompletedTasks
        {
            get { return this.handler.ShowCompletedTasks; }
        }

        public SmartView(IWorkbook workbook, ISmartView view) : base(workbook, view, view.Name)
        {
            this.smartviewOwner = view;
            this.rules = view.Rules;
            this.handler = SmartViewHandler.FromString(this.rules);

            this.Ready();
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return this.handler.IsMatch;
        }

        public override void Rebuild()
        {
            this.UpdateFilterPredicate();
            base.Rebuild();
        }
    }
}