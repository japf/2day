using System;
using System.Diagnostics;
using Windows.ApplicationModel.Store;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class WinLicenseManager
    {
        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }        
    }
}
