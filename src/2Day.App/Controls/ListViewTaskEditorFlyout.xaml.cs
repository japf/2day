using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class ListViewTaskEditorFlyout : UserControl
    {
        private readonly IWorkbook workbook;
        private static MoveType? lastMoveType;

        public event EventHandler<EventArgs<IAbstractFolder>> MoveRequest;

        public ListViewTaskEditorFlyout()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
            
            this.radioPriority.Checked += (s, e1) => this.LoadItems();
            this.radioFolder.Checked += (s, e1) => this.LoadItems();
            this.radioContext.Checked += (s, e1) => this.LoadItems();
            this.radioTag.Checked += (s, e1) => this.LoadItems();

            this.workbook = Ioc.Resolve<IWorkbook>();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.workbook.Contexts.Any() && this.workbook.Tags.Any())
            {
                // 4 columns
                this.AddColumnDefinition();
                this.AddColumnDefinition();
                this.radioTag.Visibility = Visibility.Visible;
                this.radioContext.Visibility = Visibility.Visible;
            }
            else if (this.workbook.Contexts.Any() && !this.workbook.Tags.Any())
            {
                // 3 columns
                this.AddColumnDefinition();
                this.radioTag.Visibility = Visibility.Collapsed;
                this.radioContext.Visibility = Visibility.Visible;
            }
            else if (!this.workbook.Contexts.Any() && this.workbook.Tags.Any())
            {
                // 3 columns
                this.AddColumnDefinition();
                this.radioTag.Visibility = Visibility.Visible;
                this.radioContext.Visibility = Visibility.Collapsed;
            }
            else
            {
                // 2 columns
                this.radioTag.Visibility = Visibility.Collapsed;
                this.radioContext.Visibility = Visibility.Collapsed;
            }

            if (lastMoveType.HasValue)
            {
                if (lastMoveType.Value == MoveType.Priority)
                    this.radioPriority.IsChecked = true;
                else if (lastMoveType.Value == MoveType.Folder)
                    this.radioFolder.IsChecked = true;
                else if (lastMoveType.Value == MoveType.Context && this.radioContext.Visibility == Visibility.Visible)
                    this.radioContext.IsChecked = true;
                else if (lastMoveType.Value == MoveType.Tag && this.radioTag.Visibility == Visibility.Visible)
                    this.radioTag.IsChecked = true;
                else
                    this.radioFolder.IsChecked = true;
            }
            else
            {
                this.radioFolder.IsChecked = true;
            }
            

            this.LoadItems(saveLastUse: false);
        }

        private void AddColumnDefinition()
        {
            this.rootGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
        }

        private void LoadItems(bool saveLastUse = true)
        {
            var priorities = new List<Folder>
            {
                GetPriorityFolder(FontIconHelper.IconIdPriorityNone, StringResources.TaskPriority_None, TaskPriority.None),
                GetPriorityFolder(FontIconHelper.IconIdPriorityLow, StringResources.TaskPriority_Low, TaskPriority.Low),
                GetPriorityFolder(FontIconHelper.IconIdPriorityMedium, StringResources.TaskPriority_Medium, TaskPriority.Medium),
                GetPriorityFolder(FontIconHelper.IconIdPriorityHigh, StringResources.TaskPriority_High, TaskPriority.High),
                GetPriorityFolder(FontIconHelper.IconIdPriorityStar, StringResources.TaskPriority_Star, TaskPriority.Star)
            };

            if (this.radioPriority.IsChecked.HasValue && this.radioPriority.IsChecked.Value)
            {
                this.itemsControl.ItemsSource = priorities;
                if (saveLastUse)
                    lastMoveType = MoveType.Priority;
            }
            else if (this.radioFolder.IsChecked.HasValue && this.radioFolder.IsChecked.Value)
            {
                this.itemsControl.ItemsSource = this.workbook.Folders;
                if (saveLastUse)
                    lastMoveType = MoveType.Folder;
            }
            else if (this.radioContext.IsChecked.HasValue && this.radioContext.IsChecked.Value)
            {
                this.itemsControl.ItemsSource = this.workbook.Contexts;
                if (saveLastUse)
                    lastMoveType = MoveType.Context;
            }
            else if (this.radioTag.IsChecked.HasValue && this.radioTag.IsChecked.Value)
            {
                this.itemsControl.ItemsSource = this.workbook.Tags;
                if (saveLastUse)
                    lastMoveType = MoveType.Tag;
            }
        }

        private static Folder GetPriorityFolder(int iconId, string name, TaskPriority priority)
        {
            return new PriorityFolder
            {
                IconId = iconId,
                Color = "white",
                Name = name,
                Priority = priority
            };
        }

        private void OnElementTapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.MoveRequest != null && e.OriginalSource is FrameworkElement)
            {
                var folder = ((FrameworkElement) e.OriginalSource).DataContext as IAbstractFolder;
                if (folder != null)
                    this.MoveRequest(this, new EventArgs<IAbstractFolder>(folder));
            }

            if (this.radioPriority.IsChecked.HasValue && this.radioPriority.IsChecked.Value)
                lastMoveType = MoveType.Priority;
            else if (this.radioFolder.IsChecked.HasValue && this.radioFolder.IsChecked.Value)
                lastMoveType = MoveType.Folder;
            else if (this.radioContext.IsChecked.HasValue && this.radioContext.IsChecked.Value)
                lastMoveType = MoveType.Context;
            else if (this.radioTag.IsChecked.HasValue && this.radioTag.IsChecked.Value)
                lastMoveType = MoveType.Tag;

            this.TryCloseParentPopup();
        }

        private class PriorityFolder : Folder
        {
            public TaskPriority Priority { get; set; }

            public override void MoveTasks(IEnumerable<ITask> tasks)
            {
                foreach (var task in tasks)
                {
                    task.Priority = this.Priority;
                }
            }
        }

        private enum MoveType
        {
            Priority,
            Folder,
            Context,
            Tag
        }  
    }
}
