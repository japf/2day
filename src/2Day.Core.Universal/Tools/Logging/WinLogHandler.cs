using System;
using System.Threading.Tasks;
using Windows.Storage;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Core.Universal.Tools.Logging
{
    public class WinLogHandler : ILogHandler
    {
        private ulong Size1Mb = 1024*1024;

        public async Task CheckLogRotationAsync()
        {
            try
            {
                // if log file gets too big, rename it with the .bak suffix
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(LogService.Filename);
                var properties = await file.GetBasicPropertiesAsync();
                ulong size = properties.Size;
                if (size > this.Size1Mb)
                {
                    await file.RenameAsync(LogService.Filename + ".bak");
                }
            }
            catch (Exception)
            {
                // silently fail
            }
        }

        public async Task SaveAsync(string filename, string content)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (content == null)
                throw new ArgumentNullException("content");

            await WinIsolatedStorage.AppendTextAsync(filename, content);
        }

        public async Task<string> LoadAsync(string filename)
        {
            return await WinIsolatedStorage.ReadTextAsync(filename);
        }

        public async Task DeleteAsync(string filename)
        {
            await WinIsolatedStorage.DeleteAsync(filename);
            await WinIsolatedStorage.AppendTextAsync(filename, string.Empty);
        }
    }
}
