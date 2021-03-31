using System;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FantasticBike.Shared;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace FantasticBike.Producer
{
    internal static class Program
    {
        const string AssemblerQueueName = "fantastic-bike-assembler";
        static readonly string[] bikePartNames =
        {
            "wheel", "rim", "tire", "brake", "seat", "cassette", "rear-derailleur", "front-derailleur",  
            "chain", "chainring", "crankset", "pedal", "headset", "stem", "handlerbar", "fork", "frame",
            "hub", "bottle-cage", "disk"
        };
        static readonly string[] bikeModels = { "mtb-xc", "mtb-trail", "mtb-enduro", "mtb-downhill", "bdc-aero", "bdc-endurance", "gravel", "ciclocross", "trekking", "urban" };
        
        
        static async Task Main(string[] args)
        {
            Console.Title = "FantasticBike.Producer";
            while (true)
            {
                Console.WriteLine("Specify how many bikes you want to produce:");
                var bikeAmount = ReadHowManyBikesToProduce();
                await QueueAssembleBikeMessages(bikeAmount);
                Console.WriteLine("OK, done!");
                Console.WriteLine("Produce other bikes?");
                var answer = Console.ReadLine();
                if (answer != null && !answer.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                    break;
            }
        }
        
        static async Task QueueAssembleBikeMessages(int bikeAmount)
        {
            //create the environment variable ASBConnectionString
            var connectionString = Environment.GetEnvironmentVariable("ASBConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable!");
            
            var queueClient = new QueueClient(connectionString, AssemblerQueueName);

            for (var i = 0; i < bikeAmount; i++)
            {
                var bike = ProduceBike();
                var bikeMessage = new AssembleBikeMessage(bike.Id, bike.Price, bike.Model, bike.Parts);
                var rowBikeMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bikeMessage));
                
                var nativeMessage = new Message(rowBikeMessage)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    UserProperties =
                    {
                        {"NServiceBus.EnclosedMessageTypes", typeof(AssembleBikeMessage).FullName}
                    }
                };
                
                Console.WriteLine($"Queuing bike assembly {bike.Id}");
                await queueClient.SendAsync(nativeMessage).ConfigureAwait(false);
            }
        }
        static int ReadHowManyBikesToProduce()
        {
            while (true)
            {
                var rawAmount = Console.ReadLine();
                if (int.TryParse(rawAmount, out var amount))
                    return amount;
                
                Console.WriteLine("That's not a valid amount, try again please");
            }
        }
        static Bike ProduceBike()
        {
            var bikePartGen = new Faker<BikePart>()
                .RuleFor(x => x.Id, () => Guid.NewGuid().ToString())
                .RuleFor(x => x.Name, f => f.PickRandom(bikePartNames))
                .RuleFor(x => x.Code, f => f.Commerce.Ean8());

            var bikeGen = new Faker<Bike>()
                .RuleFor(x => x.Id, () => Guid.NewGuid().ToString())
                .RuleFor(x => x.Price, f => f.Random.Number(200,10000))
                .RuleFor(x => x.Model, f => f.PickRandom(bikeModels))
                .RuleFor(u => u.Parts, f => bikePartGen.Generate(f.Random.Number(6,bikePartNames.Length)));
            
            return bikeGen.Generate();
        }
    }
}