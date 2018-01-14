using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Chartreuse.Today.Vercors.Shared.Model
{
    [DataTable("deletedTask")]
    public class VercorsDeletedTask
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "deletedId")]
        public int DeletedId { get; set; }

        [JsonProperty(PropertyName = "deletedTimestamp")]
        public long DeletedTimestamp { get; set; }
    }
}