using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Controls
{
    public class IconText : Control
    {
        private const string PART_TextBlock = "PART_TextBlock";

        private TextBlock textBlock;
        private Action templateLoaded;

        public AppIconType Icon
        {
            get { return (AppIconType)this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(AppIconType),
            typeof(IconText),
            new PropertyMetadata(AppIconType.CommonAdd, OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((IconText)d).OnIconChanged(e);
        }

        private void OnIconChanged(DependencyPropertyChangedEventArgs e)
        {
            AppIconType type = (AppIconType)e.NewValue;

            this.templateLoaded = () => this.textBlock.Text = FontIconHelper.GetSymbolCode(type);
            if (this.textBlock != null)
            {
                this.templateLoaded();
                this.templateLoaded = null;
            }
        }
        
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.textBlock = this.GetTemplateChild(PART_TextBlock) as TextBlock;
            if (this.textBlock == null)
                throw new NotSupportedException("Could not find a TextBlock in the template");

            if (this.templateLoaded != null)
                this.templateLoaded();
        }
    }
}