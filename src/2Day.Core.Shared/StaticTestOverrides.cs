using System;

namespace Chartreuse.Today.Core.Shared
{
    /// <summary>
    /// An helper class that holds static properties than can be overriden for unit testing purpose
    /// </summary>
    public static class StaticTestOverrides
    {
        public static DateTime? Now;
    }
}
