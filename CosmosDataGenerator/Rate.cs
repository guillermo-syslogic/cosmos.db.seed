namespace CosmosDataGenerator
{
    public class Rate
    {
        public RateType RateType { get; set; }
        public decimal Price { get; set; }
    }

    public enum RateType
    {
        Flat = 0,
        Hourly = 1,
        HalfDay = 2,
        Daily = 3,
        Weekly = 4,
        Monthly = 5,
        Extended = 6,
        OneDay = 7,
        TwoDay = 8,
        ThreeDay = 9,
        FourDay = 10,
        FiveDay = 11,
        AdditionalDay = 12,
        Weekend = 13
    }
}
