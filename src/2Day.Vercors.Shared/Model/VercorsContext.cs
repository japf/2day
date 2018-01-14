using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Chartreuse.Today.Vercors.Shared.Model
{
    [DataTable("context")]
    public class VercorsContext : ISyncItem
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

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }

        public VercorsContext()
        {
        }

        public VercorsContext(IContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            int syncId = 0;
            int.TryParse(context.SyncId, out syncId);

            this.ItemId = syncId;

            this.Name = context.Name;
            this.Order = context.Order;
        }

        public void UpdateTarget(IContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.SyncId = this.Id;
            context.Name = this.Name;
            context.Order = this.Order;
        }
    }
}