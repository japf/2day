
using System.Diagnostics;

namespace Chartreuse.Today.Exchange.Model
{
    [DebuggerDisplay("Name: {DisplayName} Type: {FolderType}")]
    public class ExchangeFolder
    {
        public string ServerId { get; set; }

        public string ParentId { get; set; }
        
        public string DisplayName { get; set; }
     
        public int FolderType { get; set; }
    }
}
