using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestStartupManager : IStartupManager
    {
        public TimeSpan Uptime { get; }
        public bool IsFirstLaunch { get; }

        public Task<bool> HandleStartupAsync()
        {
            return null;
        }

        public Task HandleFirstRunAsync()
        {
            return null;
        }
    }
}