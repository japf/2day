using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Chartreuse.Today.Vercors.Shared.Model
{
    [DataTable("account")]
    [DebuggerDisplay("UserId: {UserId}")]
    public class VercorsAccount
    {
        [JsonIgnore]
        public string Id
        {
            get { return this.ItemId.ToString(); }
        }

        [JsonProperty(PropertyName = "id")]
        public int ItemId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "folderEditTimestamp")]
        public long FolderEditTimestamp { get; set; }

        [JsonProperty(PropertyName = "contextEditTimestamp")]
        public long ContextEditTimestamp { get; set; }

        [JsonProperty(PropertyName = "smartviewedittimestamp")]
        public long SmartViewEditTimestamp { get; set; }

        [JsonProperty(PropertyName = "taskEditTimestamp")]
        public long TaskEditTimestamp { get; set; }

        [JsonProperty(PropertyName = "taskDeleteTimestamp")]
        public long TaskDeleteTimestamp { get; set; }
    }
}