using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.App.VoiceCommand;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;

namespace Chartreuse.Today.App.Tools
{
    public class LauncherHelper
    {
        public static void TryHandleArgs(object args)
        {
            try
            {
                if (!Ioc.HasType<IWorkbook>() || !Ioc.HasType<ITileManager>() || !Ioc.HasType<INavigationService>())
                    return;

                var workbook = Ioc.Resolve<IWorkbook>();
                var platformService = Ioc.Resolve<IPlatformService>();
                var navigationService = Ioc.Resolve<INavigationService>();

                Frame rootFrame = Window.Current.Content as Frame;
                MainPage mainPage = null;
                MainPageViewModel viewModel = null;

                if (rootFrame != null && rootFrame.Content is MainPage)
                    mainPage = (MainPage) rootFrame.Content;
                if (mainPage != null && mainPage.DataContext is MainPageViewModel)
                    viewModel = (MainPageViewModel) mainPage.DataContext;

                string arguments = args as string;

                if (args is IActivatedEventArgs)
                {
                    var activatedEventArgs = (IActivatedEventArgs) args;
                    if (activatedEventArgs.Kind == ActivationKind.VoiceCommand)
                    {
                        var cortanaService = new CortanaRuntimeService(workbook);
                        cortanaService.TryHandleActivation(new CortanaRuntimeAction(), activatedEventArgs);

                        return;
                    }                    
                }

                if (arguments == LaunchArgumentsHelper.QuickAddTask)
                {
                    navigationService.FlyoutTo(typeof(TaskPage), null);
                    return;
                }

                if (args is ToastNotificationActivatedEventArgs)
                {
                    arguments = ((ToastNotificationActivatedEventArgs)args).Argument;
                    if (!string.IsNullOrWhiteSpace(arguments) && arguments.ToLowerInvariant().StartsWith("http"))
                        platformService.OpenWebUri(arguments);
                }

                var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(workbook, arguments);
                if (descriptor.Task != null && descriptor.Type == LaunchArgumentType.EditTask)
                {
                    navigationService.FlyoutTo(typeof(TaskPage), descriptor.Task);
                }
                else if (descriptor.Folder != null && descriptor.Type == LaunchArgumentType.Select && viewModel != null)
                {
                    if (descriptor.Folder is IFolder)
                        SelectMenuItem<IFolder>(viewModel, descriptor.Folder.Id);
                    else if (descriptor.Folder is ITag)
                        SelectMenuItem<ITag>(viewModel, descriptor.Folder.Id);
                    else if (descriptor.Folder is ISmartView)
                        SelectMenuItem<ISmartView>(viewModel, descriptor.Folder.Id);
                    else if (descriptor.Folder is IView)
                        SelectMenuItem<IView>(viewModel, descriptor.Folder.Id);
                    else if (descriptor.Folder is IContext)
                        SelectMenuItem<IContext>(viewModel, descriptor.Folder.Id);
                } 
                else if (descriptor.Type == LaunchArgumentType.Sync && viewModel != null)
                {
                    viewModel.SyncCommand.Execute(null);   
                }         
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("LauncherHelper.TryHandleArgs: {0} {1}", args, ex));
            }
        }

        private static void SelectMenuItem<T>(MainPageViewModel viewModel, int id)
        {
            viewModel.SelectedMenuItem = viewModel.MenuItems.OfType<FolderItemViewModel>().FirstOrDefault(f => f.Folder is T && f.Folder.Id == id);
        }
    }
}
