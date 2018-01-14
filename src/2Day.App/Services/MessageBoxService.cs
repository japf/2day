using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.Core.Universal.Notifications;

namespace Chartreuse.Today.App.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        private readonly NavigationService navigationService;

        public MessageBoxService(NavigationService navigationService)
        {
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            this.navigationService = navigationService;
        }
        
        public async Task<DialogClosedEventArgs> ShowAsync(string title, string content, IEnumerable<string> buttons)
        {
            try
            {
                var commands = buttons.Select(b => b.ToString()).ToList();

                var dialog = new ConcurrentMessageDialog(content, title);
                foreach (var command in commands)
                {
                    dialog.Commands.Add(new UICommand(command));
                }

                IUICommand uiResult = await dialog.ShowAsync();
                if (uiResult != null && !string.IsNullOrWhiteSpace(uiResult.Label))
                    return new DialogClosedEventArgs(commands.IndexOf(uiResult.Label));
                else
                    return new DialogClosedEventArgs(-1);
            }
            catch (Exception e)
            {
                TrackingManagerHelper.Exception(e, string.Format("MessageBoxService.ShowAsync with buttons title: {0} content {1}", title, content));
                return new DialogClosedEventArgs(-1);
            }
        }

        public async Task<DialogResult> ShowAsync(string title, string content, DialogButton button = DialogButton.OK)
        {
            try
            {
                var dialog = new ConcurrentMessageDialog(content, title);

                switch (button)
                {
                    case DialogButton.OK:
                        dialog.Commands.Add(new UICommand(StringResources.General_LabelOk));
                        break;
                    case DialogButton.OKCancel:
                        dialog.Commands.Add(new UICommand(StringResources.General_LabelOk));
                        dialog.Commands.Add(new UICommand(StringResources.General_LabelCancel));
                        break;
                    case DialogButton.YesNo:
                        dialog.Commands.Add(new UICommand(StringResources.General_LabelYes));
                        dialog.Commands.Add(new UICommand(StringResources.General_LabelNo));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(button));
                }

                IUICommand result = await dialog.ShowAsync();
                if (result == null)
                    return DialogResult.Cancel;
                else if (result.Label == StringResources.General_LabelOk)
                    return DialogResult.OK;
                else if (result.Label == StringResources.General_LabelCancel)
                    return DialogResult.Cancel;
                else if (result.Label == StringResources.General_LabelYes)
                    return DialogResult.Yes;
            }
            catch (Exception e)
            {
                TrackingManagerHelper.Exception(e, string.Format("MessageBoxService.ShowAsync title: {0} content: {1}", title, content));
            }

            return DialogResult.No;
        }

        private Task<bool> ShowCustomDialogAsync<T>(string title, T dialogContent) where T : ICustomDialogContent
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (this.navigationService.HasFlyoutOpened)
                this.navigationService.CloseFlyouts();

            var tcs = new TaskCompletionSource<bool>();

            var customDialog = new FullScreenDialog
            {
                Title = title,
                BackButtonVisibility = Visibility.Visible,
                Background = Application.Current.Resources["AppFlyoutBackgroundBrush"] as SolidColorBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var frame = Window.Current.Content as Frame;
            var page = frame.Content as Page;
            var panel = page.Content as Panel;

            if (this.navigationService.HasFlyoutOpened)
            {
                var flyout = this.navigationService.PeekFlyout();
                Panel panelFlyout = null;

                if (flyout.Content is Panel)
                    panelFlyout = flyout.Content as Panel;
                else if (flyout.Content is Page)
                    panelFlyout = ((Page) flyout.Content).Content as Panel;

                if (panelFlyout != null)
                {
                    customDialog.ForceWidth((double) flyout.Size);
                    panel = panelFlyout;
                }
            }

            if (panel == null)
                throw new NotSupportedException("Could not find a panel in the page of flyout");

            customDialog.Content = dialogContent;
            
            panel.Children.Add(customDialog);

            RoutedEventHandler backButtonHandler = null;
            backButtonHandler = (s, e) =>
            {
                customDialog.BackButtonClicked -= backButtonHandler;
                tcs.SetResult(false);

                panel.Children.Remove(customDialog);
                dialogContent.OnDismissed();
                customDialog.IsOpen = false;
            };

            EventHandler requestCloseHandler = null;
            requestCloseHandler = (s, e) =>
            {
                dialogContent.RequestClose -= requestCloseHandler;
                tcs.SetResult(true);

                panel.Children.Remove(customDialog);
                dialogContent.OnDismissed();
                customDialog.IsOpen = false;
            };

            dialogContent.RequestClose += requestCloseHandler;
            customDialog.BackButtonClicked += backButtonHandler;

            customDialog.IsOpen = true;

            return tcs.Task;
        }

        public async Task<string> ShowCustomTextEditDialogAsync(string title, string placeholder, string content = null)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrEmpty(placeholder))
                throw new ArgumentNullException(nameof(placeholder));

            var dialogContent = new EditBox(placeholder, content);

            await this.ShowCustomDialogAsync(title, dialogContent);

            return dialogContent.Text;
        }
        
        // original implementation from
        // http://blogs.msdn.com/b/devosaure/archive/2012/08/28/tips-amp-tricks-erreur-171-access-denied-187-avec-messagedialog-winrt.aspx
        private class ConcurrentMessageDialog
        {
            private static readonly ConcurrentQueue<MessageDialog> dialogsQueue = new ConcurrentQueue<MessageDialog>();
            private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

            readonly MessageDialog messageDialog;

            public ConcurrentMessageDialog(string content, string title)
            {
                this.messageDialog = new MessageDialog(content, title);
                dialogsQueue.Enqueue(this.messageDialog);

            }

            public async Task<IUICommand> ShowAsync()
            {
                MessageDialog dialog;

                await semaphore.WaitAsync();

                dialogsQueue.TryDequeue(out dialog);

                var result = await dialog.ShowAsync();

                semaphore.Release(1);

                return result;
            }

            public IList<IUICommand> Commands
            {
                get { return this.messageDialog.Commands; }
            }
        }
    }
}
