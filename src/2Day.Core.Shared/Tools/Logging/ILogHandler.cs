using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Tools.Logging
{
    public interface ILogHandler
    {
        Task SaveAsync(string filename, string content);
        Task<string> LoadAsync(string filename);
        Task DeleteAsync(string filename);
    }
}