using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public class CustomRichEditBox : RichEditBox
    {
        private readonly Color hyperlinkForegroundColor;
        private readonly Color textForegroundColor;

        private Popup openLinkPopup;
        private string link;
        
        public CustomRichEditBox()
        {
            this.hyperlinkForegroundColor = this.FindResource<Color>("HyperlinkForegroundColor");
            this.textForegroundColor = this.FindResource<SolidColorBrush>("AppButtonForegroundBrush").Color;

            this.TextChanging += this.OnTextChanging;
        }

        private void OnTextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            try
            {
                this.HighlightLinks();
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Exception in CustomRichEditBox.OnTextChanging");
            }
        }
        
        private void HighlightLinks()
        {
            string textContent;
            this.Document.GetText(TextGetOptions.None, out textContent);
            int start = 0;
            int end = textContent.Length;

            ITextRange range = this.Document.GetRange(start, end);
            range.CharacterFormat.Underline = UnderlineType.None;
            range.CharacterFormat.ForegroundColor = this.textForegroundColor;

            if (this.Document.Selection == null)
                return;

            int startPosition = this.Document.Selection.StartPosition;
            int endPosition = this.Document.Selection.EndPosition;

            foreach (string uri in RegexHelper.FindUris(textContent))
            {
                int result = range.FindText(uri, end - start, FindOptions.None);

                if (result > 0)
                {
                    this.Document.Selection.SetRange(range.StartPosition, range.EndPosition);
                    range.CharacterFormat.Underline = UnderlineType.Single;
                    range.CharacterFormat.ForegroundColor = this.hyperlinkForegroundColor;

                    range.ScrollIntoView(PointOptions.None);
                }
            }

            this.Document.Selection.SetRange(startPosition, endPosition);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            try
            {
                this.HandleTap(e);
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Exception in CustomRichEditBox.OnTapped");
            }
        }

        private void HandleTap(TappedRoutedEventArgs e)
        {
            Point tappedPoint = e.GetPosition(this);
            ITextRange textRange = this.Document.GetRangeFromPoint(tappedPoint, PointOptions.ClientCoordinates);
            textRange.StartOf(TextRangeUnit.Link, true);

            if (textRange.Character != '\r' && textRange.CharacterFormat.Underline == UnderlineType.Single)
            {
                string content;
                this.Document.GetText(TextGetOptions.None, out content);

                textRange.StartPosition--;
                textRange.EndPosition++;
                while (!(textRange.Text.StartsWith(" ") || textRange.Text.StartsWith("\r")) && textRange.StartPosition > 0)
                    textRange.StartPosition--;
                while (!(textRange.Text.EndsWith(" ") || textRange.Text.EndsWith("\r")) && textRange.EndPosition < content.Length)
                    textRange.EndPosition++;

                this.link = textRange.Text;
                if (!string.IsNullOrWhiteSpace(this.link))
                {
                    this.link = this.link.Trim(' ');
                    this.link = this.link.Trim('\r');

                    if (!this.link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !this.link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        this.link = "http://" + this.link;

                    if (this.openLinkPopup == null)
                    {
                        this.openLinkPopup = new Popup
                        {
                            IsLightDismissEnabled = true,
                            Child = new HyperlinkPopupContent()
                        };
                    }

                    this.openLinkPopup.IsOpen = false;
                    ((HyperlinkPopupContent) this.openLinkPopup.Child).Hyperlink = this.link;
                    var frame = Window.Current.Content as Frame;
                    if (frame != null && frame.Content is Page)
                    {
                        var page = frame.Content as Page;

                        var position = e.GetPosition(page);
                        this.openLinkPopup.VerticalOffset = position.Y;

                        if (ResponsiveHelper.IsUsingSmallLayout())
                        {
                            this.openLinkPopup.HorizontalOffset = 15;
                            ((HyperlinkPopupContent) this.openLinkPopup.Child).MaxWidth = 292;
                        }
                        else
                        {
                            this.openLinkPopup.HorizontalOffset = position.X;
                        }
                    }

                    this.openLinkPopup.IsOpen = true;
                }
            }
        }
    }
}