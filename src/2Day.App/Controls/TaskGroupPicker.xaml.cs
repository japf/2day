using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class TaskGroupPicker : UserControl
    {
        public TaskGroupPicker()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewmodel = this.DataContext as FolderViewModelBase;
            if (viewmodel == null)
                return;

            viewmodel.Refresh();
        }
    }
}
