using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Newtonsoft.Json;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class WinIsolatedStorage
    {
        // ideally, we should have 1 semaphore per file we want to write to
        // in this simple use case, SaveAsync is only called for saving a specific file
        // so this naive approach works...
        private static readonly SemaphoreSlim SaveSemaphore = new SemaphoreSlim(1);
        
        private static bool isSaveInProgress;
        
        public static async Task SaveAsync<T>(T data, string filename, string metadata = null)
        {
            Mutex saveMutex = null;
            try
            {               
                await SaveSemaphore.WaitAsync();

                saveMutex = new Mutex(false, "2day-save-mutex");
                if (!saveMutex.WaitOne(500))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }

                isSaveInProgress = true;

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (StreamWriter streamWriter = new StreamWriter(stream))
                    {
                        await streamWriter.WriteAsync(JsonConvert.SerializeObject(data));
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("SaveAsync: {0} to: {1} in progress: {2} metadata: {3} {4}", data, filename, isSaveInProgress, metadata, ex));
            }
            finally
            {
                SafeRelease(SaveSemaphore, saveMutex);

                isSaveInProgress = false;
            }

        }

        private static void SafeRelease(SemaphoreSlim semaphore, Mutex mutex)
        {
            try
            {
                semaphore.Release(1);

                if (mutex != null)
                    mutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Safe release");
            }
        }

        public static async Task<T> RestoreAsync<T>(string filename)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

                using (var instream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader streamReader = new StreamReader(instream))
                    {
                        string json = await streamReader.ReadToEndAsync();
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                // we will update format in that case from XML to JSON :-)
                // still needed after migration of the SL version to th UWP (SL was using XML)
                TrackingManagerHelper.Exception(ex, string.Format("RestoreAsync - Error while reading {0} {1}", filename, ex));
            }
            catch (Exception ex)
            {
                // FileNotFoundException is handled right :-)
                if (!(ex is FileNotFoundException))
                    TrackingManagerHelper.Exception(ex, string.Format("RestoreAsync - Error while reading {0} {1}", filename, ex));

                return default(T);
            }

            var item = await LegacyXmlRestoreAsync<T>(filename);
            await SaveAsync(item, filename);

            return item;
        }

        private static async Task<T> LegacyXmlRestoreAsync<T>(string filename)        
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

                using (var instream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractSerializer(typeof (T));
                    return (T) serializer.ReadObject(instream);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is FileNotFoundException))
                    TrackingManagerHelper.Exception(ex, string.Format("LegacyXmlRestoreAsync - Error while reading {0} {1}", filename, ex));

                return default (T);
            }
        }

        public static async Task DeleteFolderAsync(string filename)
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(filename);

                if (folder != null)
                    await folder.DeleteAsync();
            }
            catch (Exception ex)
            {
                if (!(ex is FileNotFoundException))
                    TrackingManagerHelper.Exception(ex, string.Format("DeleteAsync - Error while deleting {0} {1}", filename, ex));
            }
        }

        public static async Task DeleteAsync(string filename)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

                if (file != null)
                    await file.DeleteAsync();
            }
            catch (Exception ex)
            {
                if (!(ex is FileNotFoundException))
                    TrackingManagerHelper.Exception(ex, string.Format("DeleteAsync - Error while deleting {0} {1}", filename, ex));
            }
        }

        public static async Task<string> ReadTextAsync(string filename)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                using (var randomAccessStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (var outputStream = randomAccessStream.GetInputStreamAt(0))
                    {
                        using (StreamReader streamReader = new StreamReader(outputStream.AsStreamForRead()))
                        {
                            return await streamReader.ReadToEndAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("ReadTextAsync - Error while reading text {0} {1}", filename, ex));
                return string.Empty;
            }
        }

        public static async Task AppendTextAsync(string filename, string content)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                using (var randomAccessStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var outputStream = randomAccessStream.GetOutputStreamAt(randomAccessStream.Size))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(outputStream.AsStreamForWrite()))
                        {
                            await streamWriter.WriteAsync(content);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("AppendTextAsync - Error while appending text {0} {1}", filename, ex));
            }
        }

        public static async Task WriteTextInFileAsync(StorageFile file, string content)
        {
            using (var randomAccessStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var outputStream = randomAccessStream.GetOutputStreamAt(randomAccessStream.Size))
                {
                    using (StreamWriter streamWriter = new StreamWriter(outputStream.AsStreamForWrite()))
                    {
                        await streamWriter.WriteAsync(content);
                    }
                }
            }
        }

        public static async Task<string> ReadTextFromFile(StorageFile file)
        {
            using (var randomAccessStream = await file.OpenAsync(FileAccessMode.Read))
            {
                using (var outputStream = randomAccessStream.GetInputStreamAt(0))
                {
                    using (StreamReader streamReader = new StreamReader(outputStream.AsStreamForRead()))
                    {
                        return await streamReader.ReadToEndAsync();
                    }
                }
            }
        }
    }
}
