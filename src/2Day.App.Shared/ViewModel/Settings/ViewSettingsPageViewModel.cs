using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class ViewSettingsPageViewModel : CollectionViewSourceSettingsViewModel, IViewSettingsPageViewModel
    {
        private ObservableCollection<ISystemView> views;

        public ViewSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService)
            : base(workbook, navigationService, messageBoxService)
        {
        }

        protected override INotifyCollectionChanged GetSource()
        {
            if (this.views == null)
                this.views = new ObservableCollection<ISystemView>(this.Workbook.Views.OrderBy(v => v.Order));
    
            return this.views;
        }

        protected override void OnOrderChanged()
        {
            this.ApplyOrder();
        }

        public override void Dispose()
        {
            this.ApplyOrder();
        }

        private void ApplyOrder()
        {
            this.Workbook.ApplyViewOrder(new List<ISystemView>(this.views));
        }
    }
}
