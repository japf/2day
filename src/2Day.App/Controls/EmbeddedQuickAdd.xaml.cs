using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class EmbeddedQuickAdd : UserControl
    {
        private readonly QuickAddTaskViewModel viewmodel;

        public QuickAddTaskViewModel ViewModel
        {
            get { return this.viewmodel; }
        }

        public EmbeddedQuickAdd()
        {
            this.InitializeComponent();

            this.viewmodel = Ioc.Build<QuickAddTaskViewModel>();
            this.viewmodel.Saved += (s, e) =>
            {
                this.viewmodel.Title = null;
                this.viewmodel.DueDate = null;
                // TreeHelper.FindVisualAncestor<MainPage>(this).HideEmbeddedQuickAdd();
            };
            
            this.DataContext = this.viewmodel;
        }

        public void UpdateTaskCreationParameters(TaskCreationParameters parameters)
        {
            this.viewmodel.UseTaskCreationParameters(parameters);
        }
    }    
}
