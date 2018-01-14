using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class CreateSmartViewViewModel : SmartViewViewModelBase
    {
        public CreateSmartViewViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ITrackingManager trackingManager)
            : base(workbook, navigationService, messageBoxService, trackingManager)
        {
            var block = new SmartViewBlockViewModel(this);
            var rule = new SmartViewRuleViewModel(block);
            block.Rules.Add(rule);

            this.Blocks.Add(block);
        }

        protected override bool SaveCore()
        {
            this.TrackingManager.TagEvent("Create SmartView", new Dictionary<string, string>());

            var smartview = this.Workbook.AddSmartView(this.Name, this.RuleContent);

            return smartview != null;
        }
    }
}
