using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class HyperlinkPopupContent : UserControl
    {
        private static readonly CoreCursor handCursor = new CoreCursor(CoreCursorType.Hand, 1);
        private static readonly CoreCursor arrowCursor = new CoreCursor(CoreCursorType.Arrow, 1);

        public string Hyperlink
        {
            get { return (string) this.GetValue(HyperlinkProperty); }
            set { this.SetValue(HyperlinkProperty, value); }
        }

        public static readonly DependencyProperty HyperlinkProperty = DependencyProperty.Register(
            "Hyperlink", 
            typeof(string), 
            typeof(HyperlinkPopupContent), 
            new PropertyMetadata(null, OnHyperlinkChanged));

        private static void OnHyperlinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HyperlinkPopupContent) d;
            control.Opacity = 0;
            control.AnimateOpacity(1, TimeSpan.FromMilliseconds(200));
        }

        public HyperlinkPopupContent()
        {
            this.InitializeComponent();

            this.border.PointerEntered += (sender, e11) =>
            {
                Window.Current.CoreWindow.PointerCursor = handCursor;
            };
            this.border.PointerExited += (sender, e11) =>
            {
                Window.Current.CoreWindow.PointerCursor = arrowCursor;
            };
            this.border.Tapped += (s, e1) =>
            {
                try
                {
                    Uri uri = SafeUri.Get(this.Hyperlink);
                    Launcher.LaunchUriAsync(uri);
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, "Exception in HyperlinkPopupContent.border.Tapped");
                }
            };            
        }
    }
}
