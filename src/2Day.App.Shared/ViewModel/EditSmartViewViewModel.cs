using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class EditSmartViewViewModel : SmartViewViewModelBase
    {
        private readonly ISmartView smartView;

        public EditSmartViewViewModel(ISmartView smartView, IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ITrackingManager trackingManager)
            : base(workbook, navigationService, messageBoxService, trackingManager)
        {
            this.smartView = smartView;

            this.Name = smartView.Name;
            this.RuleContent = smartView.Rules;

            this.LoadStringAsync(smartView.Rules);
        }

        protected override bool SaveCore()
        {
            this.TrackingManager.TagEvent("Edit SmartView", new Dictionary<string, string>());

            if (this.Workbook.SmartViews.Any(f => f.Name == this.Name && f != this.smartView))
                return false;

            this.smartView.Name = this.Name;
            this.smartView.Rules = this.RuleContent;

            return true;
        }
    }
}