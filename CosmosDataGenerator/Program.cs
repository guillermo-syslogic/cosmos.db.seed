using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDataGenerator
{
    class Program
    {
        private static readonly string databaseId = "RentalReservations";
        private static readonly string readHeavyId = "Reservations";

        static async Task Main(string[] args)
        {
            try
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                string connectionString = configuration["CosmosConnectionString"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException("Please specify a connection string in the appSettings.json file");
                }

                using (CosmosClient client = new CosmosClient(connectionString))
                {
                    var dataContainer = await CreateDBContainer(client);
                    await GenerateData(dataContainer, 100000);
                    // await QueryDataWithinPartition(dataContainer);
                    // await QueryDataWithinPartitionWithFilter(dataContainer);
                    // await QueryDataWithCrossPartitionQuery(dataContainer);
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception throw: {ex.Message}");
                throw;
            }
        }

        private static async Task<Container> CreateDBContainer(CosmosClient client)
        {
            ContainerResponse dataContainer = null;

            try
            {
                // Set up a database
                Microsoft.Azure.Cosmos.Database database = await client.CreateDatabaseIfNotExistsAsync(databaseId);

                // Container and Throughput Properties
                ContainerProperties containerProperties = new ContainerProperties(readHeavyId, "/Dealers/Locations/LocationID");
                ThroughputProperties throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(20000);

                // Create a read heavy environment
                dataContainer = await database.CreateContainerAsync(
                    containerProperties,
                    throughputProperties);              
            }
            catch (CosmosException ce)
            {
                Console.WriteLine($"Exception thrown by Cosmos DB: {ce.StatusCode}, {ce.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception thrown: {ex.Message}");
                throw;
            }

            return dataContainer;
        }

        private static async Task GenerateData(Container readContainer, int itemsToGenerate)
        {
            try
            {
                // Generate Data

                var seedItem = new Faker<Item>()
                    .RuleFor(i => i.ItemID, (fake) => Guid.NewGuid().ToString())
                    .RuleFor(i => i.Name, x => x.Commerce.ProductName());

                var seedLocations = new Faker<Location>()
                    .RuleFor(i => i.LocationID, x => Guid.NewGuid().ToString())
                    .RuleFor(i => i.Name, x => x.Address.City())
                    .RuleFor(i => i.Items, x => seedItem.Generate(new Random().Next(3,10)));
                   
                var seedDealer = new Faker<Dealer>()
                    .RuleFor(d => d.DealerID, (fake) => Guid.NewGuid().ToString())
                    .RuleFor(d => d.Name, (fake) => fake.PickRandom(new List<string> { 
                        "Wheel & sprocket", 
                        "mountain goats", 
                        "rams & rims", 
                        "down we go", 
                        "bouncy bouncy", 
                        "trails & rails", 
                        "fat wheels", 
                        "tour du brek", 
                        "Brekenridge Adventures", 
                        "Backpackers" }))
                    .RuleFor(d => d.Locations, x => seedLocations.Generate(new Random().Next(1, 5)))
                    .Generate(new Random().Next(5, 20));

                // Add to read container
                foreach (var dealer in seedDealer)
                {
                    await readContainer.CreateItemAsync(
                        dealer,
                        new PartitionKey(dealer.Locations.GetEnumerator().Current.LocationID));
                    Console.WriteLine($"DealerId: {dealer.DealerID}");
                }               
            }
            catch (CosmosException ce)
            {
                Console.WriteLine($"CosmosDB Exception thrown: {ce.StatusCode}, {ce.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception thrown: {ex.Message}");
                throw;
            }                      
        }

        private static async Task QueryDataWithinPartition(Container readContainer, string dealerId)
        {
            // Querry within a partition
            Console.WriteLine("Searching for all locations that are in dealer...");
            QueryDefinition dealerQuery = new QueryDefinition(
                string.Format("SELECT * FROM Dealers d WHERE d.DealerID = '{dealerId}'", dealerId));

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
                string.Format("SELECT * FROM Dealers d WHERE d.Locations.LocationID = {locationId}", locationId));

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
