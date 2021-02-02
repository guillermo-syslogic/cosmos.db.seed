using Newtonsoft.Json;
using System.Collections.Generic;

namespace CosmosDataGenerator
{
    public class Location
    {
        [JsonProperty("LocationID")]
        public string LocationID { get; set; }
        public string Name { get; set; }
        public IList<Item> Items { get; set; }
    }
}