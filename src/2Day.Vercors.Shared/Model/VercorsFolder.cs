using System;
using System.Diagnostics;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Chartreuse.Today.Vercors.Shared.Model
{
    [DataTable("folder")]
    [DebuggerDisplay("Name: {Name}")]
    public class VercorsFolder : ISyncItem
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

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public int Icon { get; set; }

        public VercorsFolder()
        {
        }

        public VercorsFolder(IFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            int syncId = 0;
            int.TryParse(folder.SyncId, out syncId);

            this.ItemId = syncId;

            this.Name = folder.Name;
            this.Color = folder.Color;
            this.Order = folder.Order;
            this.Icon = folder.IconId;
        }

        public void UpdateTarget(IFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            folder.SyncId = this.Id;
            folder.Name = this.Name;
            folder.Color = this.Color;
            folder.Order = this.Order;
            folder.IconId = this.Icon;
        }
    }
}