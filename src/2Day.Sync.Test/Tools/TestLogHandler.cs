using System;
using System.IO;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Sync.Test.Tools
{
    public class TestLogHandler : ILogHandler
    {
        private static readonly string folderName;

        static TestLogHandler()
        {
            folderName = "Test Traces";

            // make sure directory is empty
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);

            Directory.CreateDirectory(folderName);
        }

        public static void Initialize(string prefix)
        {
            LogService.Initialize(new TestLogHandler(), true);
            LogService.Filename = Path.Combine(folderName, $"{prefix}.xml");
            LogService.Level = LogLevel.Network;
        }

        public Task SaveAsync(string filename, string content)
        {
            File.AppendAllText(filename, content);

            return Task.FromResult(0);
        }

        public Task<string> LoadAsync(string filename)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string filename)
        {
            throw new NotImplementedException();
        }
    }
}