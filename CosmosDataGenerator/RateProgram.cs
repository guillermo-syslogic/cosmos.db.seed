using Newtonsoft.Json;
using System.Collections.Generic;

namespace CosmosDataGenerator
{
    public class RateProgram
    {
        [JsonProperty("id")]
        public string RateProgramId { get; set; }
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }
        public List<Rate> Rates { get; set; }
    }
}
