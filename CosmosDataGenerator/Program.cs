using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CosmosDataGenerator
{
    class Program
    {
        private static readonly string databaseId = "RentalReservations";
        private static readonly string locationsContainerId = "Locations";
        private static readonly string rateProgramsContainerId = "RatePrograms";

        private static readonly bool createDatabase = false;
        private static readonly bool deleteContainers = false;
        private static readonly bool createContainers = false;
        // If using addData, make sure to update objects to generate
        // Set to 0 if no new items to be added
        private static readonly bool addData = false;
        // Test data generation without database updates or turn logging on for data generation
        private static readonly bool testOrLogDataGeneration = true;

        private static readonly int dealersToGenerate = new Random().Next(5, 10);
        private static readonly int locationsPerDealerToGenerate = new Random().Next(75, 100);
        private static readonly int itemsPerLocationToGenerate = new Random().Next(50, 75);

        private static readonly int rateProgramsToGenerate = new Random().Next(3, 5);
        private static readonly int ratesPerProgramToGenerate = new Random().Next(4, 10);

        private static IConfigurationRoot configuration;

        private static readonly List<string> cosmosRateProgramIds = new List<string>();

        static async Task Main(string[] args)
        {
            if (createDatabase)
            {
                await CreateDB();
            }

            if (deleteContainers)
            {
                await DeleteContainers();
            }

            if (createContainers)
            {
                await CreateContainers();
            }
            else if (addData)
            {
                await AddData();
            }
            else if (!addData && testOrLogDataGeneration)
            {
                TestDataGeneration();
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


        private static async Task CreateDB()
        {
            using CosmosClient client = new CosmosClient(GetCosmosConnectionString());

            await client.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        private static async Task DeleteContainers()
        {
            using CosmosClient client = new CosmosClient(GetCosmosConnectionString());

            var rateProgramsContainer = client.GetContainer(databaseId, rateProgramsContainerId);
            var locationsContainer = client.GetContainer(databaseId, locationsContainerId);

            try
            {
                await locationsContainer.DeleteContainerAsync();
                await rateProgramsContainer.DeleteContainerAsync();
            }
            catch (CosmosException ce)
            {
                Console.WriteLine($"Exception thrown by Cosmos DB trying to delete containers: {ce.Message}");
                throw;
            }
        }

        private static async Task CreateContainers()
        {
            using CosmosClient client = new CosmosClient(GetCosmosConnectionString());

            await CreateContainer(client, rateProgramsContainerId);
            await CreateContainer(client, locationsContainerId);
        }

        private static async Task AddData()
        {
            using CosmosClient client = new CosmosClient(GetCosmosConnectionString());

            var rateProgramsContainer = client.GetContainer(databaseId, rateProgramsContainerId);
            var locationsContainer = client.GetContainer(databaseId, locationsContainerId);

            var ratePrograms = GenerateRateProgramsData();
            var dealers = GenerateDealersData(rateProgramsContainer);

            await InsertRateProgramsData(rateProgramsContainer, ratePrograms);
            await InsertLocationsData(locationsContainer, dealers);
        }

        private static void TestDataGeneration()
        {
            using CosmosClient client = new CosmosClient(GetCosmosConnectionString());

            var rateProgramsContainer = client.GetContainer(databaseId, rateProgramsContainerId);

            GenerateRateProgramsData();
            GenerateDealersData(rateProgramsContainer);
        }

        private static async Task<Container> CreateContainer(CosmosClient client, string containerId)
        {
            ContainerResponse dataContainer = null;

            try
            {
                var database = client.GetDatabase(databaseId);

                // Container and Throughput Properties
                ContainerProperties containerProperties = new ContainerProperties(containerId, "/partitionKey");
                ThroughputProperties throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(20000);

                // Create a read heavy environment
                dataContainer = await database.CreateContainerIfNotExistsAsync(containerProperties, throughputProperties);
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

        private static async Task InsertLocationsData(Container dataContainer, IEnumerable<Dealer> dealers)
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

        private static async Task InsertRateProgramsData(Container dataContainer, IEnumerable<RateProgram> ratePrograms)
        {
            try
            {
                // Add to read container
                foreach (var rateProgram in ratePrograms)
                {
                    rateProgram.PartitionKey = $"{rateProgram.RateProgramId}";
                    await dataContainer.CreateItemAsync(
                            rateProgram,
                            new PartitionKey(rateProgram.PartitionKey));
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

        private static IEnumerable<string> GetRatePrograms(Container ratesContainer)
        {
            if (cosmosRateProgramIds.Count == 0)
            {
                var queryText = "SELECT * FROM c";
                var queryDef = new QueryDefinition(queryText);
                var rateProgramsQuery = ratesContainer.GetItemQueryIterator<RateProgram>(queryDef);

                while (rateProgramsQuery.HasMoreResults)
                {
                    FeedResponse<RateProgram> rateProgramResponse = rateProgramsQuery.ReadNextAsync().Result;
                    foreach (var rateProgram in rateProgramResponse)
                    {
                        cosmosRateProgramIds.Add(rateProgram.RateProgramId);
                    }
                }
            }

            return cosmosRateProgramIds;
        }

        private static IEnumerable<Dealer> GenerateDealersData(Container ratesContainer)
        {
            var fakeDealerNames = new List<string> {
                "Wheel & sprocket",
                "mountain goats",
                "rams & rims",
                "down we go",
                "bouncy bouncy",
                "trails & rails",
                "fat wheels",
                "tour du brek",
                "Brekenridge Adventures",
                "Backpackers"
            };

            var fakeModelNames = new List<string>
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

            var categories = Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>();

            var fakeRatePrograms = GetRatePrograms(ratesContainer);

            var fakeItems = new Faker<Item>()
                .RuleFor(i => i.ItemID, (x) => Guid.NewGuid().ToString())
                .RuleFor(i => i.Sku, x => new Randomizer().Replace("##-######"))
                .RuleFor(i => i.Model, x => x.PickRandom(fakeModelNames))
                .RuleFor(i => i.Categories, x => x.PickRandom(categories, new Random().Next(1, 3)).ToList())
                .RuleFor(i => i.RateProgramId, x => x.PickRandom(fakeRatePrograms));

            var fakeLocations = new Faker<Location>()
                .RuleFor(i => i.LocationID, x => Guid.NewGuid().ToString())
                .RuleFor(i => i.Name, x => x.Address.City())
                .RuleFor(i => i.Items, x => fakeItems.Generate(itemsPerLocationToGenerate));

            var fakeDealers = new Faker<Dealer>()
                .RuleFor(d => d.DealerID, (x) => Guid.NewGuid().ToString())
                .RuleFor(d => d.Name, (x) => x.PickRandom(fakeDealerNames))
                .RuleFor(d => d.Locations, x => fakeLocations.Generate(locationsPerDealerToGenerate))
                .Generate(dealersToGenerate);

            if (testOrLogDataGeneration)
            {
                foreach (var dealer in fakeDealers)
                {
                    Console.WriteLine($"DealerId: {dealer.DealerID}");
                    foreach (var location in dealer.Locations)
                    {
                        Console.WriteLine($"\tLocationId: {location.LocationID}");
                        foreach (var item in location.Items)
                        {
                            Console.WriteLine($"\t\tItemId: {item.ItemID}, RateProgramId: {item.RateProgramId}");
                            foreach (var category in item.Categories)
                            {
                                Console.WriteLine($"\t\t\tCategroy: {category}");
                            }
                        }
                    }
                }
            }

            return fakeDealers;
        }

        private static IEnumerable<RateProgram> GenerateRateProgramsData()
        {
            var rateTypes = Enum.GetValues(typeof(RateType)).Cast<RateType>();

            var fakeRates = new Faker<Rate>()
                .RuleFor(i => i.RateType, x => x.PickRandom(rateTypes))
                .RuleFor(i => i.Price, x => Math.Round(new Randomizer().Decimal(new decimal(1.00), new decimal(100.00)), 2));

            var fakeRatePrograms = new Faker<RateProgram>()
                .RuleFor(i => i.RateProgramId, (x) => Guid.NewGuid().ToString())
                .RuleFor(i => i.Rates, x => fakeRates.Generate(ratesPerProgramToGenerate))
                .Generate(rateProgramsToGenerate);

            foreach (var rateProgram in fakeRatePrograms)
            {
                var uniqueRates = rateProgram.Rates.GroupBy(x => x.RateType);
                rateProgram.Rates = new List<Rate>();
                foreach(var uniqueRate in uniqueRates)
                {
                    rateProgram.Rates.Add(uniqueRate.First());
                }

                cosmosRateProgramIds.Add(rateProgram.RateProgramId);

                if (testOrLogDataGeneration)
                {
                    Console.WriteLine($"RateProgramId: {rateProgram.RateProgramId}");
                    foreach (var rateType in rateProgram.Rates)
                    {
                        Console.WriteLine($"\tRateType: {rateType.RateType}, RatePrice: {rateType.Price}");
                    }
                }
            }

            return fakeRatePrograms;
        }
    }
}
