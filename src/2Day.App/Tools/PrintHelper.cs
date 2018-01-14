using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Printing;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;

namespace Chartreuse.Today.App.Tools
{
    public class PrintHelper
    {
        private const double TaskItemHeight = 86;
        private const double GroupItemHeight = 44;

        private const double VerticalMarginPercentage = 0.03;
        private const double HorizontalMarginPercentage = 0.03;

        private static bool hasRegisteredPrintManager; // so that we can create multiple instance in unit test context

        private readonly IMessageBoxService messageBoxService;
        private readonly List<PrintPage> pages;

        private FolderItemViewModel viewmodel;
        private PrintDocument printDocument;
        private IPrintDocumentSource printDocumentSource;

        public PrintHelper(IMessageBoxService messageBoxService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));

            this.messageBoxService = messageBoxService;
            this.pages = new List<PrintPage>();

            if (Window.Current != null)
            {
                var dispatcher = Window.Current.Dispatcher;

                if (!hasRegisteredPrintManager)
                {
                    PrintManager printManager = PrintManager.GetForCurrentView();
                    printManager.PrintTaskRequested += (s, e) =>
                    {
                        var task = e.Request.CreatePrintTask(Constants.AppName, sourceRequested =>
                        {
                            sourceRequested.SetSource(this.printDocumentSource);
                        });

                        task.Completed += async (ss, ee) =>
                        {
                            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (this.printDocument != null)
                                {
                                    this.printDocument.Paginate -= this.OnPaginate;
                                    this.printDocument.GetPreviewPage -= this.OnGetPreviewPage;
                                    this.printDocument.AddPages -= this.OnAddPages;

                                    this.printDocument = null;
                                    this.printDocumentSource = null;
                                }
                            });
                        };
                    };

                    hasRegisteredPrintManager = true;
                }
            }
        }

        public async Task RequestPrintAsync(FolderItemViewModel folderItemViewModel)
        {
            this.viewmodel = folderItemViewModel;

            try
            {
                this.printDocument = new PrintDocument();
                this.printDocumentSource = this.printDocument.DocumentSource;

                this.printDocument.Paginate += this.OnPaginate;
                this.printDocument.GetPreviewPage += this.OnGetPreviewPage;
                this.printDocument.AddPages += this.OnAddPages;

                await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception ex)
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, ex.Message);
            }
        }

        private void OnAddPages(object sender, AddPagesEventArgs e)
        {
            foreach (var page in this.pages)
                this.printDocument.AddPage(page);

            PrintDocument printDoc = (PrintDocument)sender;
            printDoc.AddPagesComplete();
        }

        private PrintPage CreatePage(int startIndex, int endIndex, PrintPageDescription printPageDescription)
        {
            var content = new List<object>();
            foreach (var group in this.viewmodel.SmartCollection.Items)
            {
                content.Add(group);
                foreach (var task in group)
                {
                    content.Add(task);
                }
            }
            
            var page = new PrintPage(GetHorizontalMargin(printPageDescription), GetVerticalMargin(printPageDescription))
            {
                DataContext = content.Skip(startIndex).Take(endIndex - startIndex)
            };

            return page;
        }

        private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            PrintDocument printDoc = (PrintDocument)sender;
            printDoc.SetPreviewPage(e.PageNumber, this.pages[e.PageNumber - 1]);
        }

        private void OnPaginate(object sender, PaginateEventArgs e)
        {
            this.pages.Clear();

            List<double> heights = new List<double>();
            foreach (var group in this.viewmodel.SmartCollection.Items)
            {
                heights.Add(GroupItemHeight);
                foreach (var task in group)
                    heights.Add(TaskItemHeight);
            }

            PrintDocument printDoc = (PrintDocument)sender;
            PrintTaskOptions printingOptions = e.PrintTaskOptions;

            int startIndex = 0;

            while (heights.Count > 0)
            {
                int itemsInPage = 0;

                PrintPageDescription pageDescription = printingOptions.GetPageDescription((uint) this.pages.Count);
                double availableHeight = pageDescription.PageSize.Height - (2*GetVerticalMargin(pageDescription));

                while (heights.Count > 0 && heights[0] < availableHeight)
                {
                    availableHeight -= heights[0];
                    heights.RemoveAt(0);
                    itemsInPage++;
                }

                // create page
                this.pages.Add(this.CreatePage(startIndex, startIndex + itemsInPage, pageDescription));

                startIndex += itemsInPage;
            }

            printDoc.SetPreviewPageCount(this.pages.Count, PreviewPageCountType.Intermediate);
        }

        private static double GetHorizontalMargin(PrintPageDescription printPageDescription)
        {
            return Math.Max(printPageDescription.PageSize.Width - printPageDescription.ImageableRect.Width, printPageDescription.PageSize.Width*HorizontalMarginPercentage*2);
        }

        private static double GetVerticalMargin(PrintPageDescription printPageDescription)
        {
            return Math.Max(printPageDescription.PageSize.Height - printPageDescription.ImageableRect.Height, printPageDescription.PageSize.Height * VerticalMarginPercentage * 2);
        }
    }
}
