using System;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Services
{
    /// <summary>
    /// A service which handle startup by displaying welcome message to the user if needed
    /// </summary>
    public interface IStartupManager
    {
        /// <summary>
        /// Gets the time the app has been running so far
        /// </summary>
        TimeSpan Uptime { get; }

        /// <summary>
        /// Gets a value indicating whether it's the very first time the user launches the app
        /// </summary>
        bool IsFirstLaunch { get; }

        /// <summary>
        /// Handle startup and display messages if neeeded
        /// </summary>
        /// <returns>True if the startup can continue with an automatic sync, false otherwise</returns>
        Task<bool> HandleStartupAsync();

        Task HandleFirstRunAsync();
    }
}
