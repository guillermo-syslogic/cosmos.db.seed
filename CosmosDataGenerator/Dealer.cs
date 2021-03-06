﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace CosmosDataGenerator
{
    public class Dealer
    {
        [JsonProperty("id")]
        public string DealerID { get; set; }
        public string Name { get; set; }
        public IList<Location> Locations { get; set; }
    }
}