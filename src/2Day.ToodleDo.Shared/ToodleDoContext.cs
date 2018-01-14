using System.Diagnostics;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.ToodleDo
{
    [DebuggerDisplay("Name: {Name}")]
    public class ToodleDoContext : ISyncItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}