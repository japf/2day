using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Smart;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class CreateEditSmartViewPage : Page, ISettingsPage, IDisposable
    {
        private SmartViewViewModelBase viewmodel;

        public SettingsSizeMode Size
        {
            get { return SettingsSizeMode.Large; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }
        
        public SmartViewViewModelBase ViewModel
        {
            get { return this.viewmodel; }
        }
        
        public CreateEditSmartViewPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();
        }

        public void OnNavigatedTo(object parameter)
        {
            // occurs when we navigate to this page from a flyout
            // ie, INavigationService.Flyout(pageType, pageParameter)
            this.Initialize(parameter);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // occurs when we navigate to this page using standard navigation
            // ie, INavigationService.NavigateTo(pageType, pageParameter)
            this.Initialize(e.Parameter);
        }
        
        private void Initialize(object parameter)
        {
            var workbook = Ioc.Resolve<IWorkbook>();
            var navigationService = Ioc.Resolve<INavigationService>();
            var messageBoxService = Ioc.Resolve<IMessageBoxService>();
            var trackingManager = Ioc.Resolve<ITrackingManager>();

            if (!(parameter is ISmartView))
            {
                this.FlyoutHost.Text = StringResources.SmartView_Create;
                this.viewmodel = new CreateSmartViewViewModel(workbook, navigationService, messageBoxService, trackingManager);

                this.textboxTitle.Focus(FocusState.Programmatic);
            }
            else
            {
                this.FlyoutHost.Text = StringResources.SmartView_Edit;
                this.viewmodel= new EditSmartViewViewModel((ISmartView)parameter, workbook, navigationService, messageBoxService, trackingManager);
            }

            this.DataContext = this.viewmodel;
            this.Bindings.Update();
        }

        public void Dispose()
        {
            if (this.viewmodel != null)
                this.viewmodel.Dispose();
        }
    }

    public class CustomToggleButton : ToggleButton
    {
        protected override void OnToggle()
        {
            // prevent to directly uncheck this toggle button
            if (this.IsChecked.HasValue && this.IsChecked.Value)
                return;

            base.OnToggle();
        }
    }

    public class SmartViewRuleValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DateTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate NumericTemplate { get; set; }
        public DataTemplate EmptyTemplate { get; set; }
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate ContextTemplate { get; set; }
        public DataTemplate PriorityTemplate { get; set; }
        public DataTemplate TagTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is SmartViewRuleValueViewModel)
            {
                var viewmodel = (SmartViewRuleValueViewModel)item;
                switch (viewmodel.EditType)
                {
                    case SmartViewEditType.None:
                        return this.EmptyTemplate;
                    case SmartViewEditType.Text:
                        return this.TextTemplate;
                    case SmartViewEditType.Numeric:
                        return this.NumericTemplate;
                    case SmartViewEditType.Date:
                        return this.DateTemplate;
                    case SmartViewEditType.Folder:
                        return this.FolderTemplate;
                    case SmartViewEditType.Context:
                        return this.ContextTemplate;
                    case SmartViewEditType.Priority:
                        return this.PriorityTemplate;
                    case SmartViewEditType.Tag:
                        return this.TagTemplate;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return this.SelectTemplateCore(item, null);
        }
    }
}
