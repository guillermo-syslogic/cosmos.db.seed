using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosDataGenerator
{
    public class CosmosDataReader
    {
        public CosmosDataReader()
        {

        }
        
        public async Task Execute()
        {
            // await QueryDataWithinPartition(dataContainer);
            // await QueryDataWithinPartitionWithFilter(dataContainer);
            // await QueryDataWithCrossPartitionQuery(dataContainer);

            return;
        }

        private static async Task QueryDataWithinPartition(Container readContainer, string dealerId)
        {
            // Querry within a partition
            Console.WriteLine("Searching for all locations that are in dealer...");
            QueryDefinition dealerQuery = new QueryDefinition(
                string.Format($"SELECT * FROM Dealers d WHERE d.DealerID = '{dealerId}'", dealerId));

            FeedIterator<Dealer> dealerIterator = readContainer.GetItemQueryIterator<Dealer>(
                dealerQuery);

            while (dealerIterator.HasMoreResults)
            {
                FeedResponse<Dealer> dealerResponse = await dealerIterator.ReadNextAsync();
                foreach (var dealer in dealerResponse)
                {
                    PrintDealer(dealer);
                }

                Console.WriteLine($"Total of {dealerResponse.Count} results");
                Console.WriteLine($"This query cost: {dealerResponse.RequestCharge} RU's");
                Console.WriteLine("=======================================================");
                Console.WriteLine();
            }
        }

        private static async Task QueryDataWithinPartitionWithFilter(Container readContainer, string locationId)
        {
            // Perform a range query within a partition
            Console.WriteLine("Searching for dealer that has a given location by id");
            QueryDefinition locationFilterQuery = new QueryDefinition(
                string.Format($"SELECT * FROM Dealers d WHERE d.Locations.LocationID = {locationId}", locationId));

            FeedIterator<Dealer> locationIterator = readContainer.GetItemQueryIterator<Dealer>(
                locationFilterQuery);

            while (locationIterator.HasMoreResults)
            {
                FeedResponse<Dealer> locationFilterResponse = await locationIterator.ReadNextAsync();
                foreach (var dealer in locationFilterResponse)
                {
                    PrintDealer(dealer);
                }

                Console.WriteLine($"Total of {locationFilterResponse.Count} results");
                Console.WriteLine($"This query cost: {locationFilterResponse.RequestCharge} RU's");
                Console.WriteLine("=======================================================");
                Console.WriteLine();
            }
        }

        private static void PrintDealer(Dealer dealer)
        {
            Console.WriteLine("Hotel result");
            Console.WriteLine("====================");
            Console.WriteLine($"Id: {dealer.DealerID}");
            Console.WriteLine($"Name: {dealer.Name}");
            Console.WriteLine($"Locations: {dealer.Locations}");
            Console.WriteLine("====================");
        }

    }
}