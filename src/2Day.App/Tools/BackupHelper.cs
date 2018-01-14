using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;
using Newtonsoft.Json;

namespace Chartreuse.Today.App.Tools
{
    public class BackupHelper
    {
        private const string BackupFileName = "backup.2day";
        private const string BackupFolderName = "backup";
        private const string BackupFileExtension = ".2day";
        private const string BackupSettingsFileName = "settings.json";

        private readonly IPersistenceLayer persistenceLayer;
        private readonly IMessageBoxService messageBoxService;
        private readonly ITrackingManager trackingManager;

        public BackupHelper(IPersistenceLayer persistenceLayer, IMessageBoxService messageBoxService, ITrackingManager trackingManager)
        {
            if (persistenceLayer == null)
                throw new ArgumentNullException(nameof(persistenceLayer));
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.persistenceLayer = persistenceLayer;
            this.messageBoxService = messageBoxService;
            this.trackingManager = trackingManager;
        }

        public async Task RestoreBackupAsync()
        {
            this.trackingManager.TagEvent("Restore backup start", new Dictionary<string, string>());

            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;

                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(BackupFileExtension);

                var backupFile = await picker.PickSingleFileAsync();
                if (backupFile == null)
                    return;

                // copy backup in the local folder so that we can unzip it
                await backupFile.CopyAsync(localFolder, BackupFileName, NameCollisionOption.ReplaceExisting);

                // create backup destination folder
                await WinIsolatedStorage.DeleteFolderAsync(BackupFolderName);
                var backupFolder = await localFolder.CreateFolderAsync(BackupFolderName, CreationCollisionOption.OpenIfExists);

                // unzip archive
                await Task.Run(() => ZipFile.ExtractToDirectory(Path.Combine(localFolder.Path, BackupFileName), Path.Combine(localFolder.Path, BackupFolderName)));

                // restore settings
                var settingsFile = await backupFolder.CreateFileAsync(BackupSettingsFileName, CreationCollisionOption.OpenIfExists);
                string json = await WinIsolatedStorage.ReadTextFromFile(settingsFile);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        ApplicationData.Current.LocalSettings.Values[setting.Key] = setting.Value;
                    }
                }

                // disable sync on startup and clean auth token to make sure we don't sync invalid data right after restarting the app
                ApplicationData.Current.LocalSettings.Values[CoreSettings.SyncOnStartup] = null;
                ApplicationData.Current.LocalSettings.Values[CoreSettings.SyncAuthToken] = null;

                // copy database
                var dbFile = await backupFolder.CreateFileAsync(this.persistenceLayer.DatabaseFilename, CreationCollisionOption.OpenIfExists);
                dbFile.CopyAsync(localFolder, this.persistenceLayer.DatabaseFilename, NameCollisionOption.ReplaceExisting);

                // copy sync metatada
                var syncMetadataFile = await backupFolder.CreateFileAsync(SynchronizationMetadata.Filename, CreationCollisionOption.OpenIfExists);
                syncMetadataFile.CopyAsync(localFolder, SynchronizationMetadata.Filename, NameCollisionOption.ReplaceExisting);

                await this.messageBoxService.ShowAsync(StringResources.Message_Information, StringResources.Dialog_RestartInfo);

                this.trackingManager.TagEvent("Restore backup success", new Dictionary<string, string>());

                Application.Current.Exit();
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "RestoreBackupAsync");
                this.trackingManager.TagEvent("Restore backup exception", new Dictionary<string, string> { { "exception", ex.GetType().Name } });

                await this.messageBoxService.ShowAsync(StringResources.General_LabelError, ex.Message);
            }
        }

        public async Task CreateBackupAsync()
        {
            this.trackingManager.TagEvent("Create backup start", new Dictionary<string, string>());

            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;

                // get file pointer to database and sync metadata
                var dbFile = await localFolder.CreateFileAsync(this.persistenceLayer.DatabaseFilename, CreationCollisionOption.OpenIfExists);
                var syncMetadataFile = await localFolder.CreateFileAsync(SynchronizationMetadata.Filename, CreationCollisionOption.OpenIfExists);

                // create backup folder where we'll zip appropriate files
                await WinIsolatedStorage.DeleteFolderAsync(BackupFolderName);
                await WinIsolatedStorage.DeleteAsync(BackupFileName);
                var backupFolder = await localFolder.CreateFolderAsync(BackupFolderName, CreationCollisionOption.OpenIfExists);

                // copy database and sync metadata
                await dbFile.CopyAsync(backupFolder, this.persistenceLayer.DatabaseFilename, NameCollisionOption.ReplaceExisting);
                await syncMetadataFile.CopyAsync(backupFolder, SynchronizationMetadata.Filename, NameCollisionOption.ReplaceExisting);

                // save all settings in a json file
                var settings = new Dictionary<string, object>();
                var settingKeys = typeof(CoreSettings).GetFields(BindingFlags.Static | BindingFlags.Public).Select(p => p.Name).ToList();
                foreach (string settingKey in settingKeys)
                {
                    object settingValue = ApplicationData.Current.LocalSettings.Values[settingKey];
                    if (settingValue != null)
                        settings.Add(settingKey, settingValue);
                }
                string jsonSettings = JsonConvert.SerializeObject(settings);
                var settingsFile = await backupFolder.CreateFileAsync(BackupSettingsFileName, CreationCollisionOption.ReplaceExisting);
                await WinIsolatedStorage.WriteTextInFileAsync(settingsFile, jsonSettings);

                // zip the backup folder
                ZipFile.CreateFromDirectory(backupFolder.Path, Path.Combine(localFolder.Path, BackupFileName));

                // let users pick a place where to save the backup
                var picker = new FileSavePicker();
                picker.FileTypeChoices.Add("2Day Backup", new List<string> { BackupFileExtension });

                var backupFile = await picker.PickSaveFileAsync();
                if (backupFile == null)
                    return;

                // copy the newly created zip file to the user location
                var savedBackupFile = await localFolder.CreateFileAsync(BackupFileName, CreationCollisionOption.OpenIfExists);
                await savedBackupFile.CopyAndReplaceAsync(backupFile);

                this.trackingManager.TagEvent("Create backup success", new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "CreateBackupAsync");
                this.trackingManager.TagEvent("Create backup exception", new Dictionary<string, string> { { "exception", ex.GetType().Name } });

                await this.messageBoxService.ShowAsync(StringResources.General_LabelError, ex.Message);
            }
        }
    }
}
