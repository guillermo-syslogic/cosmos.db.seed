using Newtonsoft.Json;
using System.Collections.Generic;

namespace CosmosDataGenerator
{
    public class Item
    {
        public string ItemID { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public string RateProgramId { get; set; }
        public List<CategoryType> Categories { get; set; }
    }
}