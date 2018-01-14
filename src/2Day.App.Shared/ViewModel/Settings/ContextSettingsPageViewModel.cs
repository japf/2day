using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class ContextSettingsPageViewModel : CollectionViewSourceSettingsViewModel
    {
        private ObservableCollection<IContext> contexts;

        public ContextSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService)
            : base(workbook, navigationService, messageBoxService)
        {
        }

        protected override INotifyCollectionChanged GetSource()
        {
            if (this.contexts == null)
                this.contexts = new ObservableCollection<IContext>(this.Workbook.Contexts);

            return this.contexts;
        }

        protected override async void CreateItemExecute(string parameter)
        {
            string context = await this.MessageBoxService.ShowCustomTextEditDialogAsync(StringResources.EditContext_Add, StringResources.EditContext_Placeholder);

            if (!string.IsNullOrEmpty(context))
            {
                IContext newContext = this.Workbook.AddContext(context);
                if (newContext != null)
                    this.contexts.Add(newContext);
            }
        }

        protected override async void RemoveItemExecute(object parameter)
        {
            if (parameter is IContext)
            {
                var context = (IContext)parameter;
                var result = await this.MessageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteContextText, DialogButton.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.Workbook.RemoveContext(context.Name);
                    this.contexts.Remove(context);
                }
            }
        }

        protected override async void EditItemExecute(object parameter)
        {
            if (parameter is IContext)
            {
                var context = (IContext) parameter;

                string newName = await this.MessageBoxService.ShowCustomTextEditDialogAsync(StringResources.EditContext_Title, StringResources.EditContext_Placeholder, context.Name);
                context.TryRename(this.Workbook, newName);
            }
        }

        protected override void OnOrderChanged()
        {
            this.Workbook.ApplyContextOrder(new List<IContext>(this.contexts));
        }
    }
}
