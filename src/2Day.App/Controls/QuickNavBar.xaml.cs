using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public partial class QuickNavBar : UserControl
    {
        private const int BackgroundColorOpacity = 232;

        private readonly Action<IAbstractFolder> selectFolder;
        private List<IAbstractFolder> mainContent;
        private List<IAbstractFolder> secondaryContent;

        public QuickNavBar(Action<IAbstractFolder> selectFolder)
        {
            this.InitializeComponent();

            this.selectFolder = selectFolder;
        }

        public void Show(IEnumerable<IAbstractFolder> folders)
        {
            // awalys update background is case theme changed
            Color color = this.FindResource<SolidColorBrush>("BackgroundBrush").Color;
            color.A = BackgroundColorOpacity;
            this.layoutRoot.Background = new SolidColorBrush(color);

            var source = folders.ToList();

            var main = new List<IAbstractFolder>(source.Where(f => f is IView && !(f is ITag)));
            main.AddRange(source.Where(f => f is IContext));
            main.AddRange(source.Where(f => f is ITag));

            var secondary = new List<IAbstractFolder>(source.Where(f => f is IFolder));

            if (this.mainContent == null || !main.ContainsSameContentAs(this.mainContent))
            {
                this.mainItemsControl.ItemsSource = main;
                this.mainContent = main;
            }

            if (this.secondaryContent == null || !secondary.ContainsSameContentAs(this.secondaryContent))
            {
                this.secondaryItemsControl.ItemsSource = secondary;
                this.secondaryContent = secondary;
            }
        }
        
        private void OnFolderTap(object sender, TappedRoutedEventArgs e)
        {    
            var button = (Button)sender;
            if (button.DataContext is IAbstractFolder)
            {
                var folder = (IAbstractFolder)button.DataContext;

                this.selectFolder(folder);
            }
        }
    }
}
