using System;
using Chartreuse.Today.Core.Shared.Model;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Chartreuse.Today.Vercors.Shared.Model
{
    [DataTable("smartview")]
    public class VercorsSmartView
    {
        [JsonIgnore]
        public string Id
        {
            get { return this.ItemId; }
        }

        [JsonProperty(PropertyName = "id")]
        public string ItemId { get; set; }

        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }

        [JsonProperty(PropertyName = "rules")]
        public string Rules { get; set; }

        public VercorsSmartView()
        {
        }

        public VercorsSmartView(ISmartView smartView)
        {
            if (smartView == null)
                throw new ArgumentNullException("context");

            this.ItemId = smartView.SyncId;
            this.Name = smartView.Name;
            this.Order = smartView.Order;
            this.Rules = smartView.Rules;
        }

        public void UpdateTarget(ISmartView smartView)
        {
            if (smartView == null)
                throw new ArgumentNullException("context");

            smartView.SyncId = this.Id;
            smartView.Name = this.Name;
            smartView.Order = this.Order;
            smartView.Rules = this.Rules;
        }
    }
}