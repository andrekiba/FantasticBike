using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FantasticBike.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FantasticBike.Buy
{
    public class FunctionEndpoint
    {
        const string AssemblerQueueName = "fantastic-bike-assembler";
        static readonly string[] bikePartNames =
        {
            "wheel", "rim", "tire", "brake", "seat", "cassette", "rear-derailleur", "front-derailleur",  
            "chain", "chainring", "crankset", "pedal", "headset", "stem", "handlerbar", "fork", "frame",
            "hub", "bottle-cage", "disk"
        };
        static readonly string[] bikeModels = 
        { 
            "mtb-xc", "mtb-trail", "mtb-enduro", "mtb-downhill", "bdc-aero",
            "bdc-endurance", "gravel", "ciclocross", "trekking", "urban" 
        };
        
        readonly QueueClient assemblerQueue;
        public FunctionEndpoint(IConfiguration configuration)
        {
            assemblerQueue = new QueueClient(configuration.GetValue<string>("ASBConnectionString"), AssemblerQueueName);
        }
        
        [FunctionName("Buy")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "buy")] HttpRequest req,
            ILogger log)
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
            await assemblerQueue.SendAsync(nativeMessage).ConfigureAwait(false);

            return new AcceptedResult();
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
