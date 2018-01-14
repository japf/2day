using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Universal.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views
{
    public class BrushDescriptor
    {
        public string Key { get; set; }
        public SolidColorBrush Brush { get; set; }
    }

    public sealed partial class DebugPage : Page
    {
        private readonly IWorkbook workbook;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly IPersistenceLayer persistence;
        private readonly IMessageBoxService messageBoxService;

        public DebugPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.workbook = Ioc.Resolve<IWorkbook>();
            this.synchronizationManager = Ioc.Resolve<ISynchronizationManager>();
            this.persistence = Ioc.Resolve<IPersistenceLayer>();
            this.messageBoxService = Ioc.Resolve<IMessageBoxService>();

            this.DataContext = Ioc.Build<DebugPageViewModel>();

            this.Loaded += this.OnLoaded;

            var descriptors = new List<BrushDescriptor>();
            foreach (var key in ApplicationBrushes.GetKeys())
            {
                descriptors.Add(new BrushDescriptor
                {
                    Key = key,
                    Brush = this.FindResource<SolidColorBrush>(key)
                });
            }

            this.ItemsControlBrushes.ItemsSource = descriptors;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.tsToastBackgroundSync.IsOn = this.workbook.Settings.GetValue<bool>(CoreSettings.BackgroundToast);
            this.tsDebugMode.IsOn = this.workbook.Settings.GetValue<bool>(CoreSettings.DebugEnabled);

            this.cbLogLevels.ItemsSource = LogService.AvailableLevels;
            this.cbLogLevels.SelectedItem = this.workbook.Settings.GetValue<LogLevel>(CoreSettings.LogLevel);

            await this.LoadLogs();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var level = (LogLevel)this.cbLogLevels.SelectedItem;

            this.workbook.Settings.SetValue(CoreSettings.LogLevel, level);
            LogService.Level = level;
        }

        private async Task LoadLogs()
        {
            await LogService.SaveAsync();
            var content = await LogService.LoadAsync();
            this.rtbLog.Blocks.Clear();

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run { Text = content });
            this.rtbLog.Blocks.Add(paragraph);
        }

        private void OnButtonResetSyncTapped(object sender, TappedRoutedEventArgs e)
        {
            this.synchronizationManager.Reset(false);
        }

        private void OnButtonAddTestDataTapped(object sender, TappedRoutedEventArgs e)
        {
            using (this.workbook.WithTransaction())
            {
                SampleData.AddBigSampleData(this.workbook);
            }
        }

        private void OnButtonAddRealDataTapped(object sender, TappedRoutedEventArgs e)
        {
            using (this.workbook.WithTransaction())
            {
                SampleData.AddRealSampleData(this.workbook);
            }
        }
        
        private async void OnButtonSetAlarm(object sender, TappedRoutedEventArgs e)
        {
            if (!this.workbook.Folders.Any())
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, "No folder");
                return;

            }

            var task = new Core.Shared.Model.Impl.Task
            {
                Title = "Example alarm",
                Alarm = DateTime.Now.AddSeconds(15),
                Folder = this.workbook.Folders[0]
            };
        }

        private async void OnButtonClearLogs(object sender, TappedRoutedEventArgs e)
        {
            await LogService.DeleteAsync();
            await this.LoadLogs();
        }

        private void OnToggleDebugMode(object sender, RoutedEventArgs e)
        {
            this.workbook.Settings.SetValue(CoreSettings.DebugEnabled, this.tsDebugMode.IsOn);
        }

        private void OnToggleToastBackgroundSync(object sender, RoutedEventArgs e)
        {
            this.workbook.Settings.SetValue(CoreSettings.BackgroundToast, this.tsToastBackgroundSync.IsOn);
        }        
    }
}
