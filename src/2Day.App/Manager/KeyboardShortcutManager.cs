using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Manager.Shortcut;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Manager
{
    public class KeyboardShortcutManager
    {
        private readonly ITrackingManager trackingManager;
        private readonly List<KeyboardShortcutBase> shortcuts;

        public KeyboardShortcutManager(IWorkbook workbook, Frame rootFrame, INavigationService navigationService, ITrackingManager trackingManager)
        {
            this.trackingManager = trackingManager;
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (rootFrame == null)
                throw new ArgumentNullException(nameof(rootFrame));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.shortcuts = new List<KeyboardShortcutBase>
            {
                new NavigateBackwardShortcut(rootFrame, navigationService),
                new SaveChangesShortcut(rootFrame, navigationService),
                new CreateTaskShortcut(rootFrame, navigationService, workbook),
                new SelectAllTasksShortcut(rootFrame, navigationService),
                new SaveAndContinueShortcut(rootFrame, navigationService),
                new EscapeMainPageShortcut(rootFrame, navigationService),
                new MenuNavigateShorcut(rootFrame, navigationService),
                new EscapeMainPageShortcut(rootFrame, navigationService),
                new DeleteSelectionShorcut(rootFrame, navigationService),
                new StartSpeechRecognitionShortcut(rootFrame, navigationService),
                new StartSyncShortcut(rootFrame, navigationService),
                new SearchShortcut(rootFrame, navigationService),
                new ToggleNavMenuShortcut(rootFrame, navigationService),
                new CopyPasteShortcut(rootFrame, navigationService, workbook),
                new PrintShortcut(rootFrame, navigationService),
                new DebugShorcut(rootFrame, navigationService),
                new AddSubTaskShortcut(rootFrame, navigationService)
            };

            rootFrame.KeyUp += this.OnRootFrameKeyUp;
        }

        private void OnRootFrameKeyUp(object sender, KeyRoutedEventArgs e)
        {
            foreach (KeyboardShortcutBase shortcut in this.shortcuts)
            {
                if(shortcut.CanExecute(e))
                {
                    bool result = shortcut.Execute(e);
                    this.trackingManager.TagEvent(
                        "Keyboard shortcut", 
                        new Dictionary<string, string>
                        {
                            { "Type", shortcut.GetType().Name },
                            { "Success", result.ToString() },
                        });

                    break;
                }
            }
        }
    }
}
