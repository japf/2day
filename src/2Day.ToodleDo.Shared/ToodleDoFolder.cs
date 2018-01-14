using System.Diagnostics;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.ToodleDo
{
    [DebuggerDisplay("Name: {Name}")]
    public class ToodleDoFolder : ISyncItem
    {
        public string Id { get; set; }
        public bool Private { get; set; }
        public bool Archived { get; set; }
        public string Name { get; set; }
    }
}