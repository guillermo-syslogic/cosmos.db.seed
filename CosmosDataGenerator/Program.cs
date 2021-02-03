using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CosmosDataGenerator
{
    class Program
    {
        private static readonly string databaseId = "RentalReservations";
        private static readonly string containerId = "Locations";
        private static readonly bool insertData = true;
        private static readonly bool createContainer = true;

        private static IConfigurationRoot configuration;

        static async Task Main(string[] args)
        {
            try
            {

                var dealers = GenerateData(new Random().Next(5, 10));
                
                if(createContainer)
                {
                    using (CosmosClient client = new CosmosClient(GetCosmosConnectionString()))
                    {
                        await CreateDBContainer(client);
                    }
                }

                if (insertData)
                {
                    using (CosmosClient client = new CosmosClient(GetCosmosConnectionString()))
                    {
                        var dataContainer = client.GetContainer(databaseId, containerId);
                        //var dataContainer = await CreateDBContainer(client);
                        await InsertData(dataContainer, dealers);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception throw: {ex.Message}");
                throw;
            }
        }

        private static string GetCosmosConnectionString()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration["CosmosConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Please specify a connection string in the appSettings.json file");
            }

            return connectionString;
        }


        private static async Task CreateDB(CosmosClient client)
        {
            // Set up a database
            Microsoft.Azure.Cosmos.Database database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        private static async Task<Container> CreateDBContainer(CosmosClient client)
        {
            ContainerResponse dataContainer = null;

            try
            {

                var database = client.GetDatabase(databaseId);

                // Container and Throughput Properties
                ContainerProperties containerProperties = new ContainerProperties(containerId, "/partitionKey");
                ThroughputProperties throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(20000);

                // Create a read heavy environment
                dataContainer = await database.CreateContainerAsync(containerProperties, throughputProperties);
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

        private static async Task InsertData(Container dataContainer, IEnumerable<Dealer> dealers)
        {
            try
            {

                // Add to read container
                foreach (var dealer in dealers)
                {
                    foreach (var location in dealer.Locations)
                    {
                        location.DealerID = dealer.DealerID;
                        location.PartitionKey = $"{dealer.DealerID}:{location.LocationID}";

                        await dataContainer.CreateItemAsync(
                            location,
                            new PartitionKey(location.PartitionKey));
                    }
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

            return;
        }

        private static IEnumerable<Dealer> GenerateData(int itemsToGenerate)
        {

            var bikeNames = new List<string>
            {
                "Stache 7",
                "Superfly 20",
                "T80 24-Speed Midstep BLX",
                "Verve 3 Women's",
                "Conduit+",
                "C720+ SE",
                "Domane ALR 3",
                "Domane ALR 4",
                "CrossRip 1",
                "Domane S 4",
                "Farley EX 9.8",
                "Ibiza 21-Speed Midstep BLX"
            };


            var categories = new List<string>
            {
                "Road bikes",
                "Hybrid bikes",
                "Mountain bikes",
                "Electric bikes",
                "Diamant bikes",
                "Electra bikes",
                "Kids' bikes",
                "City bikes"
            };

            var fakeItems = new Faker<Item>()
                .RuleFor(i => i.ItemID, (x) => Guid.NewGuid().ToString())
                .RuleFor(i => i.Sku, x => new Randomizer().Replace("##-######"))
                .RuleFor(i => i.Model, x => x.PickRandom(categories))
                .RuleFor(i => i.Name, x => x.PickRandom(bikeNames));


            var fakeLocations = new Faker<Location>()
                .RuleFor(i => i.LocationID, x => Guid.NewGuid().ToString())
                .RuleFor(i => i.Name, x => x.Address.City())
                .RuleFor(i => i.Items, x => fakeItems.Generate(new Random().Next(3, 10)));

            var fakeDealers = new Faker<Dealer>()
                .RuleFor(d => d.DealerID, (x) => Guid.NewGuid().ToString())
                .RuleFor(d => d.Name, (x) => x.PickRandom(new List<string> {
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
                .RuleFor(d => d.Locations, x => fakeLocations.Generate(new Random().Next(1, 5)))
                .Generate(itemsToGenerate);

            foreach (var dealer in fakeDealers)
            {

                Console.WriteLine($"DealerId: {dealer.DealerID}");
                foreach (var location in dealer.Locations)
                {
                    Console.WriteLine($"\tLocationId: {location.LocationID}");
                }

                //var dealerJSON = JsonSerializer.Serialize(fakeDealers);
                //Console.WriteLine(dealerJSON);
                //Console.ReadKey();

            }

            return fakeDealers;
        }

    }
}
