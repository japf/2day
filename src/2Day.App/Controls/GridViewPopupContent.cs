using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Controls
{
    public class GridViewPopupContent : ItemsControl
    {
        private GridView gridView;
        private WrapGrid wrapGrid;
        
        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(GridViewPopupContent),
            new PropertyMetadata(null));

        public ICommand CreateItemCommand
        {
            get { return (ICommand)this.GetValue(CreateItemCommandProperty); }
            set { this.SetValue(CreateItemCommandProperty, value); }
        }

        public static readonly DependencyProperty CreateItemCommandProperty = DependencyProperty.Register(
            "CreateItemCommand",
            typeof(ICommand),
            typeof(GridViewPopupContent),
            new PropertyMetadata(null));

        public string CreateItemText
        {
            get { return (string)this.GetValue(CreateItemTextProperty); }
            set { this.SetValue(CreateItemTextProperty, value); }
        }

        public static readonly DependencyProperty CreateItemTextProperty = DependencyProperty.Register(
            "CreateItemText",
            typeof(string),
            typeof(GridViewPopupContent),
            new PropertyMetadata(null));

        public DataTemplate CreateItemTemplate
        {
            get { return (DataTemplate)this.GetValue(CreateItemTemplateProperty); }
            set { this.SetValue(CreateItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty CreateItemTemplateProperty = DependencyProperty.Register(
            "CreateItemTemplate",
            typeof(DataTemplate),
            typeof(GridViewPopupContent),
            new PropertyMetadata(null));

        public GridViewPopupContent()
        {
            this.SizeChanged += this.OnSizeChanged;
        }
        
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetupWrapGrid();
        }

        private void SetupWrapGrid()
        {
            if (this.wrapGrid == null)
                return;

            bool singleColumn;
            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                double w = this.ActualWidth;
                this.wrapGrid.ItemWidth = w;
                //this.popupBorder.Width = w;

                this.wrapGrid.MaximumRowsOrColumns = 1;
                singleColumn = true;
            }
            else
            {
                double w = this.ActualWidth;
                this.wrapGrid.ItemWidth = w /2 - 12;
                //this.popupBorder.Width = w;
                this.wrapGrid.MaximumRowsOrColumns = 2;
                singleColumn = false;
            }

            var enumerable = this.ItemsSource as IEnumerable<object>;
            if (enumerable != null)
            {
                this.gridView.SelectionChanged -= this.OnGridViewSelectionChanged;
                var itemsSource = singleColumn ? new List<object>(enumerable) : enumerable.AlternateTwoColumns();

                if (this.CreateItemCommand != null)
                    itemsSource.Add(this.CreateItemText);

                this.gridView.ItemsSource = itemsSource;
                this.gridView.SelectionChanged += this.OnGridViewSelectionChanged;
            }
        }

        protected override void OnApplyTemplate()
        {
            this.gridView = this.GetTemplateChild("PART_GridView") as GridView;
            if (this.gridView == null)
                throw new ArgumentException("Template must contains a GridView named PART_GridView");
            
            this.gridView.Loaded += (s, e) =>
            {
                if (this.wrapGrid == null)
                {
                    this.wrapGrid = TreeHelper.FindVisualChild<WrapGrid>(this.gridView);
                    this.SetupWrapGrid();
                }

                // select SelectedItem between the ComboBox and the inner GridView
                this.gridView.SelectionChanged -= this.OnGridViewSelectionChanged;
                this.gridView.SelectedItem = this.SelectedItem;
                this.gridView.SelectionChanged += this.OnGridViewSelectionChanged;

                this.gridView.ItemTemplateSelector = new GridViewPopupContentItemSelector
                {
                    Template = this.ItemTemplate,
                    CreateItemTemplate = this.CreateItemTemplate
                };
            };

            base.OnApplyTemplate();
        }
        
        private void OnGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var popup = this.FindParent<Popup>();
            if (popup != null)
                popup.IsOpen = false;

            if (this.gridView.SelectedItem as string == this.CreateItemText)
                this.CreateItemCommand.Execute(null);
            else
                this.SelectedItem = this.gridView.SelectedItem;
        }

        private class GridViewPopupContentItemSelector : DataTemplateSelector
        {
            public DataTemplate Template { get; set; }

            public DataTemplate CreateItemTemplate { get; set; }

            protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            {
                if (item is string)
                    return this.CreateItemTemplate;
                else
                    return this.Template;
            }
        }
    }
}
