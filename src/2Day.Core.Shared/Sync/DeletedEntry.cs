using System.Runtime.Serialization;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Sync
{
    [DataContract(Namespace = "http://www.2day-app.com/schemas")]
    public struct DeletedEntry
    {
        [DataMember]
        public int FolderId { get; private set; }
        
        [DataMember]
        public string SyncId { get; private set; }

        [DataMember]
        public TaskProperties Properties { get; private set; }

        public DeletedEntry(int folderId, string syncId)
            : this()
        {
            this.FolderId = folderId;
            this.SyncId = syncId;
            this.Properties = TaskProperties.None;
        }

        public DeletedEntry(int folderId, string syncId, TaskProperties properties)
            : this()
        {
            this.FolderId = folderId;
            this.SyncId = syncId;
            this.Properties = properties;
        }
    }
}