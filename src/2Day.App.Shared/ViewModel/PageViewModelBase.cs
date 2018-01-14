using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public abstract class PageViewModelBase : ViewModelBase, IDisposable, IPageViewModel
    {
        private readonly ICommand goBackCommand;
        private readonly ICommand pinCommand;
        private readonly ICommand saveCommand;
        private readonly ICommand deleteCommand;

        private readonly INavigationService navigationService;
        private readonly IWorkbook workbook;

        private bool isBusy;
        private string busyText;

        public bool IsSpeechRecognitionAvailable
        {
            get { return SupportedCultures.IsSpeechRecognitionSupported; }
        }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }
            protected set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    this.RaisePropertyChanged("IsBusy");

                    this.OnIsBusyChanged();
                }
            }
        }

        public string BusyText
        {
            get
            {
                return this.busyText;
            }
            protected set
            {
                if (this.busyText != value)
                {
                    this.busyText = value;
                    this.RaisePropertyChanged("BusyText");
                }
            }
        }

        public ICommand SaveCommand
        {
            get { return this.saveCommand; }
        }

        public ICommand PinCommand
        {
            get { return this.pinCommand; }
        }

        public ICommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        public ICommand GoBackCommand
        {
            get { return this.goBackCommand; }
        }

        public virtual bool CanGoBack
        {
            get { return this.navigationService.CanGoBack; }
        }

        public virtual bool CanPin
        {
            get { return false; }
        }

        public virtual bool IsPinned
        {
            get { return false; }
        }

        public virtual bool CanDelete
        {
            get { return false; }
        }

        protected IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        protected ISettings Settings
        {
            get { return this.Workbook.Settings; }
        }

        protected INavigationService NavigationService
        {
            get { return this.navigationService; }
        }
        
        protected PageViewModelBase(IWorkbook workbook, INavigationService navigationService)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            this.workbook = workbook;
            this.navigationService = navigationService;

            this.goBackCommand = new RelayCommand(this.GoBackExecute);
            this.pinCommand = new RelayCommand(this.PinExecute);
            this.saveCommand = new RelayCommand(this.SaveExecute);
            this.deleteCommand = new RelayCommand(this.DeleteExecute);
        }

        protected virtual void OnIsBusyChanged()
        {
        }

        /// <summary>
        /// Offers an opportunity to cancel go back navigation in subclasses
        /// </summary>
        /// <returns>True if the go back navigation must be cancelled, false otherwise</returns>
        protected virtual Task<bool> CancelGoBackAsync()
        {
            return Task.FromResult(false);
        }

        protected virtual async void GoBackExecute()
        {
            bool cancel = await this.CancelGoBackAsync();
            if (!cancel)
                this.NavigationService.GoBack();
        }

        protected virtual void DeleteExecute()
        {
        }

        protected virtual void PinExecute()
        {
        }

        protected virtual void SaveExecute()
        {
        }

        protected virtual IDisposable ExecuteBusyAction(string description)
        {
            this.BusyText = description;
            this.IsBusy = true;

            return new BusyAction(this);
        }

        private class BusyAction : IDisposable
        {
            private PageViewModelBase owner;

            public BusyAction(PageViewModelBase owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                if (this.owner != null)
                {
                    this.owner.IsBusy = false;
                    this.owner.BusyText = string.Empty;

                    this.owner = null;
                }
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
