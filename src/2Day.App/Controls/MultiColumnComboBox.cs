using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Controls
{
    public class MultiColumnComboBox : ComboBox
    {
        private GridView gridView;
        private Popup popup;
        private WrapGrid wrapGrid;
        private Border popupBorder;

        public DataTemplate DropDownItemTemplate
        {
            get { return (DataTemplate) this.GetValue(DropDownItemTemplateProperty); }
            set { this.SetValue(DropDownItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty DropDownItemTemplateProperty = DependencyProperty.Register(
            "DropDownItemTemplate", 
            typeof(DataTemplate), 
            typeof(MultiColumnComboBox), 
            new PropertyMetadata(null));

        public MultiColumnComboBox()
        {
            this.SizeChanged += this.OnSizeChanged;
        }
        
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (this.popup != null)
                this.popup.IsOpen = false;

            base.OnLostFocus(e);
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
                this.popupBorder.Width = w;

                this.wrapGrid.MaximumRowsOrColumns = 1;
                singleColumn = true;
            }
            else
            {
                double w = this.ActualWidth + 50;
                this.wrapGrid.ItemWidth = w /2 - 6;
                this.popupBorder.Width = w;

                this.wrapGrid.MaximumRowsOrColumns = 2;
                singleColumn = false;
            }

            var enumerable = this.ItemsSource as IEnumerable<object>;
            if (enumerable != null)
            {
                this.gridView.SelectionChanged -= this.OnGridViewSelectionChanged;
                this.gridView.ItemsSource = singleColumn ? enumerable : enumerable.AlternateTwoColumns();
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
            };
            
            this.popup = this.GetTemplateChild("Popup") as Popup;
            if (this.popup == null)
                throw new ArgumentException("Template must contains a GridView named Popup");

            this.popup.Opened += this.OnPopupOpened;

            this.popupBorder = this.GetTemplateChild("PopupBorder") as Border;
            if (this.popupBorder == null)
                throw new ArgumentException("Template must contains a Border named PopupBorder");

            base.OnApplyTemplate();
        }

        private void OnPopupOpened(object sender, object e)
        {
            try
            {
                // check if vertical offset of the popup must be adjusted to keep content on screen
                // that can be necessary if DPI is set to 125% for example or screen height is a bit small
                this.popupBorder.Measure(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));
                this.popupBorder.Arrange(new Rect(0, 0, this.popupBorder.DesiredSize.Width, this.popupBorder.DesiredSize.Height));

                var parentPage = TreeHelper.FindVisualAncestor<Page>(this);
                var popupOrigin = this.TransformToVisual(parentPage).TransformPoint(new Point(0, this.ActualHeight));

                if (popupOrigin.Y + this.popupBorder.DesiredSize.Height > Window.Current.Bounds.Height)
                {
                    this.popup.VerticalOffset = -(this.popupBorder.DesiredSize.Height - (Window.Current.Bounds.Height - popupOrigin.Y));
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Failed to position popup");
            }
        }

        private void OnGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItem = this.gridView.SelectedItem;
            this.popup.IsOpen = false;
        }
    }
}
