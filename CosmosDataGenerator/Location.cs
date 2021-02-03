using Newtonsoft.Json;
using System.Collections.Generic;

namespace CosmosDataGenerator
{
    public class Location
    {

        [JsonProperty("id")]
        public string LocationID { get; set; }
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        public string DealerID { get; set; }
        public string Name { get; set; }
        public IList<Item> Items { get; set; }
    }
}