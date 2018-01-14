using System;
using System.Collections.Specialized;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public abstract class CollectionViewSourceSettingsViewModel : PageViewModelBase
    {
        private readonly INotifyCollectionChanged cvs;
        private readonly ICommand createItemCommand;
        private readonly ICommand removeItemCommand;
        private readonly ICommand editItemCommand;
        private readonly IMessageBoxService messageBoxService;

        public INotifyCollectionChanged CollectionView
        {
            get { return this.cvs; }
        }

        public ICommand CreateItemCommand
        {
            get { return this.createItemCommand; }
        }

        public ICommand RemoveItemCommand
        {
            get { return this.removeItemCommand; }
        }

        public ICommand EditItemCommand
        {
            get { return this.editItemCommand; }
        }

        protected IMessageBoxService MessageBoxService
        {
            get { return this.messageBoxService; }
        }

        protected CollectionViewSourceSettingsViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService) : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException("messageBoxService");

            this.messageBoxService = messageBoxService;

            var source = this.GetSource();
            source.CollectionChanged += this.OnCollectionChanged;

            this.cvs = source;
            this.createItemCommand = new RelayCommand<string>(this.CreateItemExecute);
            this.removeItemCommand = new RelayCommand<object>(this.RemoveItemExecute);
            this.editItemCommand = new RelayCommand<object>(this.EditItemExecute);
        }

        protected virtual void CreateItemExecute(string parameter)
        {
        }

        protected virtual void RemoveItemExecute(object parameter)
        {
        }

        protected virtual void EditItemExecute(object parameter)
        {
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // a drag/drop will remove and then add an item
            if (e.Action == NotifyCollectionChangedAction.Add)
                this.OnOrderChanged();
        }

        protected abstract INotifyCollectionChanged GetSource();

        protected abstract void OnOrderChanged();
    }
}