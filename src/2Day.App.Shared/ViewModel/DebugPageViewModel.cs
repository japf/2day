using System;
using System.Collections.Generic;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class DebugPageViewModel : PageViewModelBase
    {
        private readonly IDatabaseContext databaseContext;
        private readonly List<AppIconType> icons;

        // private readonly ObservableCollection<DebugItemViewModel> items;

        public override bool CanGoBack
        {
            get { return true; }
        }

        public List<AppIconType> Icons
        {
            get { return this.icons; }
        }

        public string DatabasePath
        {
            get { return this.databaseContext.FullPathFile; }
        }

        public string Culture
        {
            get { return CultureInfo.CurrentUICulture.ToString(); }
        }

        public string SyncUserId
        {
            get { return this.Workbook.Settings.GetValue<string>(CoreSettings.SyncUserId); }
        }

        /*public ObservableCollection<DebugItemViewModel> Items
        {
            get { return this.items; }
        }*/

        public DebugPageViewModel(IWorkbook workbook, INavigationService navigationService, IDatabaseContext databaseContext)
            : base(workbook, navigationService)
        {
            if (databaseContext == null)
                throw new ArgumentNullException(nameof(databaseContext));

            this.databaseContext = databaseContext;
            this.icons = new List<AppIconType>();

            var enums = Enum.GetValues(typeof (AppIconType));
            foreach (object value in enums)
            {
                this.icons.Add((AppIconType)value);
            }

            /*
            this.items = new ObservableCollection<DebugItemViewModel>();
            for (int i = 0; i < 10; i++)
            {
                this.items.Add(new DebugItemViewModel { Name = "item " + i, Value = 0});
            }
            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(200)};
            var random = new Random((int) DateTime.Now.Ticks);
            timer.Tick += (s, e) =>
            {
                var item = this.items[random.Next(items.Count)];
                item.Value = random.Next(20);
            };
            timer.Start();
            */
        }
    }
}