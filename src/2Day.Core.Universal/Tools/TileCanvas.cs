using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public class TileCanvas : Canvas
    {
        public string ImageSource
        {
            get { return (string)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", 
            typeof(string), 
            typeof(TileCanvas), 
            new PropertyMetadata(null, ImageSourceChanged));

        public double ImageOpacity
        {
            get { return (double) this.GetValue(ImageOpacityProperty); }
            set { this.SetValue(ImageOpacityProperty, value); }
        }

        public static readonly DependencyProperty ImageOpacityProperty = DependencyProperty.Register(
            "ImageOpacity", 
            typeof(double), 
            typeof(TileCanvas), 
            new PropertyMetadata(1.0, ImageOpacityChanged));

        private Border opacityBorder;

        private Size lastActualSize;
        private int width;
        private int height;
        private BitmapImage bitmapSource;

        public TileCanvas()
        {
            this.LayoutUpdated += this.OnLayoutUpdated;
        }

        private Border CreateOpacityBorder()
        {
            return new Border { Background = this.FindResource<SolidColorBrush>("AppHubSectionBackgroundBrush") };
        }

        private void OnLayoutUpdated(object sender, object o)
        {
            var newSize = new Size(this.ActualWidth, this.ActualHeight);
            if (this.lastActualSize != newSize)
            {
                this.lastActualSize = newSize;
                this.Rebuild();
            }
        }

        private static void ImageOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = ((TileCanvas) d);

            if (self.opacityBorder != null)
                self.opacityBorder.Opacity =  1 - (double) e.NewValue;
        }


        private static void ImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileCanvas)d).ImageSourceChanged();
        }

        private async void ImageSourceChanged()
        {
            this.Children.Clear();

            string source = this.ImageSource;
            if (!string.IsNullOrEmpty(source))
            {
                this.bitmapSource = new BitmapImage();

                var image = new Image { Stretch = Stretch.UniformToFill };
                this.bitmapSource.ImageOpened += this.ImageOnImageOpened;
                this.bitmapSource.ImageFailed += this.ImageOnImageFailed;

                try
                {
                    if (source.StartsWith("/"))
                    {
                        // embedded resource
                        this.bitmapSource.UriSource = SafeUri.Get("ms-appx://" + source, UriKind.Absolute);
                    }
                    else
                    {
                        // file stored in local app folder
                        // try to open the file
                        StorageFile storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(source);
                        using (IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
                        {
                            await this.bitmapSource.SetSourceAsync(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, "Exception TileCanvas.ImageSourceChanged");
                }

                image.Source = this.bitmapSource;

                if (this.Children.Count == 0)
                    this.Children.Add(image);
            }

            if (this.opacityBorder != null)
                this.opacityBorder.Opacity = 1 - this.ImageOpacity;
        }

        private void ImageOnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            
        }

        private void ImageOnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            var bitmapImage = (BitmapImage)sender;
            bitmapImage.ImageOpened -= this.ImageOnImageOpened;

            this.width = this.bitmapSource.PixelWidth;
            this.height = this.bitmapSource.PixelHeight;
            
            if (this.width == 0 || this.height == 0)
                return;

            this.Rebuild();
        }

        private void Rebuild()
        {
            if (string.IsNullOrEmpty(this.ImageSource))
                return;

            if (this.width == 0 || this.height == 0)
                return;
            
            this.Children.Clear();
            for (int x = 0; x < this.ActualWidth; x += this.width)
            {
                for (int y = 0; y < this.ActualHeight; y += this.height)
                {
                    Image image = new Image
                    {
                        Stretch = Stretch.UniformToFill,
                        Source = this.bitmapSource
                    };

                    Canvas.SetLeft(image, x);
                    Canvas.SetTop(image, y);
                    this.Children.Add(image);
                }
            }

            if (this.Children.Count == 1)
            {
                Image image = (Image) this.Children[0];
                image.Width = this.ActualWidth;
                image.Height = this.ActualHeight;
            }

            this.opacityBorder = this.CreateOpacityBorder();
            this.opacityBorder.Width = this.ActualWidth;
            this.opacityBorder.Height = this.ActualHeight;
            this.opacityBorder.Opacity = 1 - this.ImageOpacity;

            this.Children.Add(this.opacityBorder);

            this.Clip = new RectangleGeometry { Rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight) };
        }
    }
}
