using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class SmartViewSettingsPageViewModel : CollectionViewSourceSettingsViewModel
    {
        private ObservableCollection<ISmartView> smartViews;

        public SmartViewSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService)
            : base(workbook, navigationService, messageBoxService)
        {
            this.Workbook.SmartViewAdded += this.OnSmartViewAdded;
        }

        public override void Dispose()
        {
            this.Workbook.SmartViewAdded -= this.OnSmartViewAdded;
        }

        private void OnSmartViewAdded(object sender, EventArgs<ISmartView> e)
        {
            this.smartViews.Clear();
            foreach (var smartView in this.Workbook.SmartViews)
                this.smartViews.Add(smartView);
        }

        protected override INotifyCollectionChanged GetSource()
        {
            if (this.smartViews == null)
                this.smartViews = new ObservableCollection<ISmartView>(this.Workbook.SmartViews);

            return this.smartViews;
        }

        protected override void CreateItemExecute(string parameter)
        {
            this.NavigationService.FlyoutTo(ViewLocator.CreateEditSmartViewPage);
        }

        protected override async void RemoveItemExecute(object parameter)
        {
            if (parameter is ISmartView)
            {
                var smartview = (ISmartView)parameter;
                var result = await this.MessageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteSmartViewText, DialogButton.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.Workbook.RemoveSmartView(smartview.Name);
                    this.smartViews.Remove(smartview);
                }
            }
        }

        protected override void EditItemExecute(object parameter)
        {
            if (parameter is ISmartView)
            {
                var smartview = (ISmartView) parameter;
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditSmartViewPage, smartview);
            }
        }

        protected override void OnOrderChanged()
        {
            this.Workbook.ApplySmartViewOrder(new List<ISmartView>(this.smartViews));
        }
    }
}
