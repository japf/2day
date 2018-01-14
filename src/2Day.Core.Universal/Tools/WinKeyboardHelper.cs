using System;
using Windows.System;
using Windows.UI.Core;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class WinKeyboardHelper
    {
        public static bool IsKeyDown(VirtualKey key)
        {
            try
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(key);
                return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
