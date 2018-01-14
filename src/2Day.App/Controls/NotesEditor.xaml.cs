using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class NotesEditor : UserControl
    {
        private readonly INavigationService navigationService;

        public TaskViewModelBase TaskViewModel
        {
            get { return (TaskViewModelBase) this.GetValue(TaskViewModelProperty); }
            set { this.SetValue(TaskViewModelProperty, value); }
        }

        public static readonly DependencyProperty TaskViewModelProperty = DependencyProperty.Register(
            "TaskViewModel", 
            typeof(TaskViewModelBase), 
            typeof(NotesEditor), 
            new PropertyMetadata(null, OnTaskViewModelChanged));

        public bool SkipDisposeOnUnload { get; set; }

        public NotesEditor()
        {
            this.InitializeComponent();

            this.rtbNotes.Loaded += this.OnRichEditBoxLoaded;
            this.rtbNotes.Paste += this.OnRichEditBoxPaste;
            this.rtbNotes.TextChanged += this.OnRichEditBoxTextChanged;
            
            this.Unloaded += this.OnUnloaded;

            // when a flyout close, refresh the content of the edit box
            // that is needed when the user edits the notes in the secondary flyout
            this.navigationService = Ioc.Resolve<INavigationService>();
            this.navigationService.FlyoutClosing += this.OnFlyoutClosing;
        }

        private void OnFlyoutClosing(object sender, EventArgs e)
        {
            this.SetRichEditBoxContentFromViewModel();
        }

        private static void OnTaskViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (NotesEditor) d;

            editor.TaskViewModel.NoteSpeechRecognitionCompleted += editor.OnSpeechRecognitionCompleted;
            editor.TaskViewModel.NoteCleared += editor.OnNoteCleared;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (!this.SkipDisposeOnUnload)
            {
                // do not dispose if the control is shown in the TaskPage as this page is cached
                this.TaskViewModel.NoteSpeechRecognitionCompleted -= this.OnSpeechRecognitionCompleted;
                this.TaskViewModel.NoteCleared -= this.OnNoteCleared;
                this.navigationService.FlyoutClosing -= this.OnFlyoutClosing;
            }
        }

        private void OnSpeechRecognitionCompleted(object sender, EventArgs<string> e)
        {
            string actualContent = this.ReadRichEditBoxContent();
            if (string.IsNullOrWhiteSpace(actualContent))
                actualContent = e.Item;
            else
                actualContent = actualContent + Environment.NewLine + e.Item;

            this.rtbNotes.Document.SetText(TextSetOptions.None, actualContent);
        }
        
        private void OnNoteCleared(object sender, EventArgs e)
        {
            this.rtbNotes.Document.SetText(TextSetOptions.None, string.Empty);
        }

        private void OnWebViewLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.webView = (WebView)sender;

            if (this.TaskViewModel != null && !string.IsNullOrEmpty(this.TaskViewModel.Note))
                this.webView.NavigateToString(this.TaskViewModel.Note);
        }

        public void SetRichEditBoxContentFromViewModel()
        {
            if (!string.IsNullOrEmpty(this.TaskViewModel.Note))
                this.rtbNotes.Document.SetText(TextSetOptions.None, this.TaskViewModel.Note);
            else
                this.rtbNotes.Document.SetText(TextSetOptions.None, string.Empty);
        }

        private void OnRichEditBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.SetRichEditBoxContentFromViewModel();
        }

        private void OnRichEditBoxTextChanged(object sender, RoutedEventArgs e)
        {
            this.TaskViewModel.Note = this.ReadRichEditBoxContent();
        }

        private async void OnRichEditBoxPaste(object sender, TextControlPasteEventArgs e)
        {
            try
            {
                DataPackageView dataPackageView = Clipboard.GetContent();
                string text = null;
                string html = null;

                if (dataPackageView.Contains(StandardDataFormats.Text))
                    text = await dataPackageView.GetTextAsync();

                if (dataPackageView.Contains(StandardDataFormats.Html))
                    html = await dataPackageView.GetHtmlFormatAsync();

                // lots of hack, but basically, it's just for OneNote...
                if (this.rtbNotes.Document.Selection != null && html != null && html.Contains("Version:1.0") && !html.Contains("SourceURL") && html.Contains("<meta name=Generator content=\"Microsoft OneNote"))
                {
                    this.rtbNotes.Document.Selection.TypeText(text);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Exception while pasting data: " + ex);
                e.Handled = false;
            }
        }

        private string ReadRichEditBoxContent()
        {
            string content;

            this.rtbNotes.Document.GetText(TextGetOptions.UseCrlf, out content);

            return content;
        }

        private void OnTextBlockEditTapped(object sender, TappedRoutedEventArgs e)
        {
            this.TaskViewModel.Note = this.TaskViewModel.Note.StripHtml();

            this.rtbNotes.Document.SetText(TextSetOptions.None, this.TaskViewModel.Note);
            this.TaskViewModel.EvaluateHtmlNote();
        }
    }
}
