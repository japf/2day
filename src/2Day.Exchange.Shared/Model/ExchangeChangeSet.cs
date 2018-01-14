using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
    [DataContract]
    [DebuggerDisplay("Added: {AddedTasks.Count} Modified: {ModifiedTasks.Count} Deleted: {DeletedTasks.Count}")]
    public class ExchangeChangeSet
    {
        [DataMember]
        public List<ExchangeTask> AddedTasks
        {
            get;
            set;
        }

        [DataMember]
        public List<ExchangeTask> ModifiedTasks
        {
            get;
            set;
        }

        [DataMember]
        public List<ServerDeletedAsset> DeletedTasks
        {
            get;
            set;
        }

        public ExchangeChangeSet()
        {
            this.AddedTasks = new List<ExchangeTask>();
            this.ModifiedTasks = new List<ExchangeTask>();
            this.DeletedTasks = new List<ServerDeletedAsset>();
        }
    }
}