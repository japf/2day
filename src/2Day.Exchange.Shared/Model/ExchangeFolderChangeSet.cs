using System.Collections.Generic;

namespace Chartreuse.Today.Exchange.Model
{
    public class ExchangeFolderChangeSet
    {
        public List<ExchangeFolder> AddedFolders { get; private set; }
        public List<ExchangeFolder> ModifiedFolders { get; private set; }
        public List<ExchangeFolder> DeletedFolders { get; private set; } 

        public ExchangeFolderChangeSet()
        {
            this.AddedFolders = new List<ExchangeFolder>();
            this.ModifiedFolders = new List<ExchangeFolder>();
            this.DeletedFolders = new List<ExchangeFolder>();
        }


    }
}
