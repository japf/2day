using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public class ResponsivePivot : ItemsControl
    {
        private Border smallContent;
        private Border largeContent;
        private Grid grid;
        private Pivot pivot;
        private List<PivotContent> pivotItems;
        private bool isInSmallMode;
        private double maxRelativeWidth;

        public static double GetRelativeSize(DependencyObject obj)
        {
            return (double)obj.GetValue(RelativeSizeProperty);
        }

        public static void SetRelativeSize(DependencyObject obj, double value)
        {
            obj.SetValue(RelativeSizeProperty, value);
        }

        public static readonly DependencyProperty RelativeSizeProperty = DependencyProperty.RegisterAttached(
            "RelativeSize", 
            typeof(double), 
            typeof(ResponsivePivot), 
            new PropertyMetadata(1.0));

        public ResponsivePivot()
        {
            this.Loaded += (s, e) =>
            {
                if (this.pivot != null)
                    this.pivot.SelectedIndex = 0;
            };
        }

        protected override void OnApplyTemplate()
        {
            this.smallContent = this.GetTemplateChild("PART_SmallContent") as Border;
            this.largeContent = this.GetTemplateChild("PART_LargeContent") as Border;
            this.grid = this.GetTemplateChild("PART_Grid") as Grid;
            this.pivot = this.GetTemplateChild("PART_Pivot") as Pivot;

            this.pivotItems = this.Items.Select(i => GetPivotContent((PivotItem)i)).ToList();
            if (this.pivotItems.Count > 0)
                this.maxRelativeWidth = Math.Min(1.0, this.pivotItems.Max(i => i.RelativeWidth));

            this.Items.Clear();
            foreach(var pivotItem in this.pivotItems)
            {
                this.grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pivotItem.RelativeWidth / this.maxRelativeWidth, GridUnitType.Star) });
            }

            this.AdjustLayout(this.ActualWidth);
            this.SizeChanged += this.OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AdjustLayout(e.NewSize.Width);
        }

        private void AdjustLayout(double width)
        {
            if (width < ResponsiveHelper.MinWidth && !this.isInSmallMode)
            {
                // small mode
                this.smallContent.Visibility = Visibility.Visible;
                this.largeContent.Visibility = Visibility.Collapsed;

                foreach (var child in this.grid.Children)
                {
                    ((ResponsivePivotItem)child).Content = null;
                }
                this.pivot.Items.Clear();
                this.grid.Children.Clear();

                foreach (var pivotItem in this.pivotItems)
                {
                    this.pivot.Items.Add(new PivotItem { Header = pivotItem.Header, Content = pivotItem.Content });
                }
                this.pivot.Opacity = 0;
                this.pivot.AnimateOpacity(1, TimeSpan.FromMilliseconds(300));
                    
                this.isInSmallMode = true;
            }
            else if (width >= ResponsiveHelper.MinWidth && this.isInSmallMode)
            {
                // large mode
                this.smallContent.Visibility = Visibility.Collapsed;
                this.largeContent.Visibility = Visibility.Visible;

                foreach (var pivotItem in this.pivot.Items)
                {
                    ((PivotItem)pivotItem).Content = null;
                }
                this.pivot.Items.Clear();
                this.grid.Children.Clear();

                int column = 0;
                foreach (var pivotItem in this.pivotItems)
                {
                    var content = new ResponsivePivotItem
                                        {
                                            Header = pivotItem.Header,
                                            Content = pivotItem.Content,
                                            ShowSeparator = column < this.pivotItems.Count - 1
                                        };

                    this.grid.Children.Add(content);
                    Grid.SetColumn(content, column);

                    column++;
                }
                this.grid.Opacity = 0;
                this.grid.AnimateOpacity(1, TimeSpan.FromMilliseconds(300));

                this.isInSmallMode = false;
            }
        }

        private PivotContent GetPivotContent(PivotItem pivotItem)
        {
            var pivotContent = new PivotContent(
                (string)pivotItem.Header, 
                (UIElement)pivotItem.Content,
                GetRelativeSize(pivotItem));

            pivotItem.Content = null;

            return pivotContent;
        }

        private struct PivotContent
        {
            public string Header { get; private set; }

            public double RelativeWidth { get; private set; }

            public UIElement Content { get; private set; }

            public PivotContent(string name, UIElement content, double relativeSize = 1) : this()
            {
                this.Header = name;
                this.Content = content;
                this.RelativeWidth = relativeSize;
            }
        }
    }
}
