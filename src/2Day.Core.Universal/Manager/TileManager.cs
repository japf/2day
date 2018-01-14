using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.Core.Universal.Manager
{
    public class TileManager : ITileManager
    {
        // buffer task changes to 2s before sending changes to live tiles
        // so that we're not updating live tiles too many times when a group 
        // of tasks changes in a short period of time (like deleting 20 tasks at once)
        private const int UpdateBufferMs = 2000;

        private const string quickAddTaskTileId = "quickAddTile";

        private readonly IWorkbook workbook;
        private readonly ITrackingManager trackingManager;
        private readonly INotificationService notificationService;
        private readonly bool isBackground;
        private readonly DispatcherTimer updateTileTimer;

        private readonly List<SecondaryTile> secondaryTiles;

        public bool IsQuickAddTileEnabled
        {
            get { return SecondaryTile.Exists(quickAddTaskTileId); }
        }
        
        public TileManager(IWorkbook workbook, ITrackingManager trackingManager, INotificationService notificationService, bool isBackground)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.workbook = workbook;
            this.trackingManager = trackingManager;
            this.notificationService = notificationService;
            this.isBackground = isBackground;
            this.secondaryTiles = new List<SecondaryTile>();

            if (!this.isBackground)
            {
                this.updateTileTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(UpdateBufferMs)};

                this.updateTileTimer.Tick += (s, e) =>
                {
                    this.updateTileTimer.Stop();
                    this.UpdateTiles();
                };

                this.workbook.TaskAdded += (s, e) =>
                {
                    this.updateTileTimer.Stop();
                    this.updateTileTimer.Start();
                };
                this.workbook.TaskRemoved += (s, e) =>
                {
                    this.updateTileTimer.Stop();
                    this.updateTileTimer.Start();
                };
                this.workbook.TaskChanged += (s, e) =>
                {
                    this.updateTileTimer.Stop();
                    this.updateTileTimer.Start();
                };
                this.workbook.FolderChanged += (s, e) =>
                {
                    this.updateTileTimer.Stop();
                    this.updateTileTimer.Start();
                };
            }
        }

        private void TrackEvent(string category, string section)
        {
            var source = this.isBackground ? TrackingSource.TileBackground : TrackingSource.Tile;
            this.trackingManager.Event(source, category, section);
        }

        public async Task LoadSecondaryTilesAsync()
        {
            var tiles = await SecondaryTile.FindAllAsync();
            this.secondaryTiles.Clear();
            this.secondaryTiles.AddRange(tiles);
        }

        public void UpdateTiles()
        {
            try
            {
                // update main tile
                var todayView = this.workbook.Views.FirstOrDefault(v => v.ViewKind == ViewKind.Today);
                if (todayView != null)
                {
                    var todayTasks = this.PickTasks(todayView);
                    this.SetTileContent(StringResources.SystemView_TitleToday, todayTasks, TileUpdateManager.CreateTileUpdaterForApplication());

                    bool badgeSet = false;
                    string badgeFolderValue = this.workbook.Settings.GetValue<string>(CoreSettings.BadgeValue);
                    if (!string.IsNullOrWhiteSpace(badgeFolderValue))
                    {
                        var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, badgeFolderValue);
                        if (descriptor != null && descriptor.Folder != null)
                        {
                            var folder = descriptor.Folder;
                            this.SetBadgeValue(BadgeUpdateManager.CreateBadgeUpdaterForApplication(), this.PickTasks(folder).Count);
                            badgeSet = true;
                        }
                    }

                    if (!badgeSet)
                        this.SetBadgeValue(BadgeUpdateManager.CreateBadgeUpdaterForApplication(), 0);
                }
                else
                {
                    this.TrackEvent("Update main tile", "Could not find view today");
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Error while updating main tile");
            }

            if (this.secondaryTiles == null)
            {
                this.TrackEvent("Update secondary tiles", "Secondary tiles are not loaded, skipping update");
                return;
            }

            foreach (var secondaryTile in this.secondaryTiles)
            {
                try
                {
                    TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(secondaryTile.TileId);

                    IAbstractFolder folder = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, secondaryTile.TileId).Folder;
                    ITask task = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, secondaryTile.TileId).Task;

                    if (folder != null)
                    {
                        var title = folder.Name;
                        var folderTasks = this.PickTasks(folder);

                        this.SetTileContent(title, folderTasks, tileUpdater, folder);
                        this.SetBadgeValue(BadgeUpdateManager.CreateBadgeUpdaterForSecondaryTile(secondaryTile.TileId), folderTasks.Count);
                    }
                    else if (task != null)
                    {
                        this.SetTileContent(task, tileUpdater);
                    }
                    else if (secondaryTile.TileId != quickAddTaskTileId)
                    {
                        // task that is linked to no folder and no task...
                        this.SetTileContent(StringResources.Message_LiveTileRemoveMe, new List<ITask>(), tileUpdater);
                    }
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, "Update secondary tiles");
                }
            }
        }

        public async Task<bool> PinAsync(ITask task)
        {
            string tileId = LaunchArgumentsHelper.GetArgEditTask(task);

            var tile = new SecondaryTile(
                tileId,
                task.Title,
                tileId,
                SafeUri.Get("ms-appx:///Assets/Logo150.png"),
                TileSize.Wide310x150);

            tile.VisualElements.Wide310x150Logo = SafeUri.Get("ms-appx:///Assets/Wide310x150Logo.png");

            tile.RoamingEnabled = true;

            bool isPinned = await tile.RequestCreateForSelectionAsync(GetPlacement(), Placement.Above);
            if (isPinned)
            {
                if (this.updateTileTimer != null)
                    this.updateTileTimer.Start();

                this.secondaryTiles.Add(tile);
                this.notificationService?.ShowNotification(string.Format(StringResources.Notification_TaskPinnedFormat, task.Title), ToastType.Info);

                return true;
            }

            return false;
        }

        public async Task<bool> PinAsync(IAbstractFolder abstractFolder)
        {
            string tileId = LaunchArgumentsHelper.GetArgSelectFolder(abstractFolder);

            var tile = new SecondaryTile(
                tileId,
                abstractFolder.Name,
                tileId,
                SafeUri.Get("ms-appx:///Assets/Logo150.png"),
                TileSize.Wide310x150);

            tile.VisualElements.Wide310x150Logo = SafeUri.Get("ms-appx:///Assets/Wide310x150Logo.png");
            tile.VisualElements.Square310x310Logo = SafeUri.Get("ms-appx:///Assets/Square310x310Logo.png");
            tile.VisualElements.BackgroundColor = Colors.Transparent;

            tile.RoamingEnabled = true;

            bool isPinned = await tile.RequestCreateForSelectionAsync(GetPlacement(), Placement.Below);
            if (isPinned)
            {
                if (this.updateTileTimer != null)
                    this.updateTileTimer.Start();

                this.secondaryTiles.Add(tile);
                this.notificationService?.ShowNotification(string.Format(StringResources.Notification_FolderPinnedFormat, abstractFolder.Name), ToastType.Info);

                return true;
            }

            return false;
        }

        public async Task<bool> PinQuickAdd()
        {
            var tile = new SecondaryTile(
                quickAddTaskTileId,
                StringResources.Tile_QuickAdd,
                LaunchArgumentsHelper.QuickAddTask,
                SafeUri.Get("ms-appx:///Assets/Square150x150LogoQuickAdd.png"),
                TileSize.Square150x150);
            
            bool isPinned = await tile.RequestCreateForSelectionAsync(GetPlacement(), Placement.Above);
            if (isPinned)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> UnpinQuickAdd()
        {
            var tile = new SecondaryTile(quickAddTaskTileId);
            bool result = await tile.RequestDeleteForSelectionAsync(GetPlacement(), Placement.Below);

            return result;
        }

        public bool IsPinned(ITask task)
        {
            try
            {
                return SecondaryTile.Exists(LaunchArgumentsHelper.GetArgEditTask(task));
            }
            catch (Exception ex)
            {
                LogService.Log("TileManager", $"Exception in IsPinned: {ex}");
                return false;
            }
        }

        public bool IsPinned(IAbstractFolder folder)
        {
            try
            {
                return SecondaryTile.Exists(LaunchArgumentsHelper.GetArgSelectFolder(folder));
            }
            catch (Exception ex)
            {
                LogService.Log("TileManager", $"Exception in IsPinned: {ex}");
                return false;
            }
        }

        public async Task<bool> UnpinAsync(ITask task)
        {
            if (this.IsPinned(task))
            {
                string id = LaunchArgumentsHelper.GetArgEditTask(task);
                var tile = new SecondaryTile(id);
                bool result = await tile.RequestDeleteForSelectionAsync(GetPlacement(), Placement.Below);

                if (result)
                {
                    this.secondaryTiles.Remove(t => t.TileId == id);
                    this.notificationService?.ShowNotification(string.Format(StringResources.Notification_TaskUnpinnedFormat, task.Title), ToastType.Info);
                }

                return result;
            }

            return false;
        }

        public async Task<bool> UnpinAsync(IAbstractFolder folder)
        {
            if (this.IsPinned(folder))
            {
                string id = LaunchArgumentsHelper.GetArgSelectFolder(folder);
                var tile = new SecondaryTile(id);
                bool result = await tile.RequestDeleteForSelectionAsync(GetPlacement(), Placement.Below);

                if (result)
                {
                    this.secondaryTiles.Remove(t => t.TileId == id);
                    this.notificationService?.ShowNotification(string.Format(StringResources.Notification_FolderUnpinnedFormat, folder.Name), ToastType.Info);
                }

                return result;
            }

            return false;
        }

        private IList<ITask> PickTasks(IAbstractFolder abstractFolder)
        {
            return TaskPicker.SelectTasks(abstractFolder, this.workbook.Settings).ToList();
        }

        private void SetTileContent(string title, IList<ITask> tasks, TileUpdater tileUpdater, IAbstractFolder folder = null)
        {
            var tileNotification = NotificationContentBuilder.CreateTileNotification(title, tasks, folder);
            if (tileNotification != null)
                tileUpdater.Update(new TileNotification(tileNotification));
        }

        private void SetTileContent(ITask task, TileUpdater tileUpdater)
        {
            var tileNotification = NotificationContentBuilder.CreateTileNotificationForTask(task);
            if (tileNotification != null)
                tileUpdater.Update(new TileNotification(tileNotification));
        }

        private void SetBadgeValue(BadgeUpdater badgeUpdater, int value)
        {
            // create a string with the badge template xml
            string badgeXmlString = string.Format("<badge value='{0}'/>", value);
            XmlDocument badgeDOM = new XmlDocument();
            try
            {
                // create a DOM
                badgeDOM.LoadXml(badgeXmlString);

                // load the xml string into the DOM, catching any invalid xml characters 
                BadgeNotification badge = new BadgeNotification(badgeDOM);

                // create a badge notification
                badgeUpdater.Update(badge);
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, $"Set badge error loading xml: {badgeXmlString}");                
            }
        }

        private static Rect GetPlacement()
        {
            var bounds = Window.Current.Bounds;
            return new Rect(new Point(), new Point(bounds.Width, bounds.Height / 2 - 200));
        }

        /*
        private async void OnTaskRemoved(object sender, EventArgs<ITask> e)
        {
            // task has been removed: unpin it if needed
            ITask task = e.Item;
            if (this.IsPinned(task))
            {
                await this.UnpinAsync(task);
            }
        }

        private async void OnFolderRemoved(object sender, EventArgs<IFolder> e)
        {
            // folder has been removed: unpin it if needed
            IFolder folder = e.Item;
            if (this.IsPinned(folder))
            {
                await this.UnpinAsync(folder);
            }
        }

        private async void OnFolderChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ISystemView && e.PropertyName == "IsEnabled")
            {
                var view = (ISystemView)sender;
                if (!view.IsEnabled)
                {
                    // if this value goes from true to false, make sure the folder is unpinned
                    // because if the tile will not work otherwise since the folder is not visible
                    await this.UnpinAsync(view);
                }
            }
        }

        private async void OnContextRemoved(object sender, EventArgs<IContext> e)
        {
            IContext folder = e.Item;
            if (this.IsPinned(folder))
            {
                await this.UnpinAsync(folder);
            }
        }

        private async void OnTagRemoved(object sender, EventArgs<ITag> e)
        {
            ITag folder = e.Item;
            if (this.IsPinned(folder))
            {
                await this.UnpinAsync(folder);
            }
        }*/
    }
}
