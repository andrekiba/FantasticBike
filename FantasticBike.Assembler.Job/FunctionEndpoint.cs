using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FantasticBike.Shared;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FantasticBike.Assembler.Job
{
    public class FunctionEnpoint
    {
        const string EndpointName = "fantastic-bike-assembler";
        readonly NServiceBus.FunctionEndpoint endpoint;
        readonly Faker faker = new Faker();
        public FunctionEnpoint(NServiceBus.FunctionEndpoint endpoint) => this.endpoint = endpoint;
        
        [Disable]
        [FunctionName(EndpointName)]
        public Task Run(
            [ServiceBusTrigger(queueName: "%NServiceBus:EndpointName%")] Message message, 
            ILogger logger, 
            ExecutionContext executionContext) =>
            endpoint.Process(message, executionContext, logger);

        [FunctionName("AssembleBike")]
        public async Task AssembleBike(
            [ServiceBusTrigger("fantastic-bike-assembler", Connection = "AzureWebJobsServiceBus")] AssembleBikeMessage assembleBikeMessage,
            [ServiceBus("fantastic-bike-shipper", Connection = "AzureWebJobsServiceBus")]IAsyncCollector<Message> collector,
            ILogger logger,
            IConfiguration configuration,
            MessageReceiver messageReceiver,
            ExecutionContext executionContext
            )
        {
            logger.LogWarning($"Handling {nameof(AssembleBikeMessage)} in {nameof(AssembleBike)}.");
            await Task.Delay(TimeSpan.Parse(configuration.GetValue<string>("FakeWorkDuration")));

            var shipBikeMessage = new ShipBikeMessage(assembleBikeMessage.Id, faker.Address.FullAddress());
            var rowShipBikeMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(shipBikeMessage));
                
            var nativeMessage = new Message(rowShipBikeMessage)
            {
                MessageId = Guid.NewGuid().ToString(),
                UserProperties =
                {
                    {"NServiceBus.EnclosedMessageTypes", typeof(ShipBikeMessage).FullName}
                }
            };
            
            await collector.AddAsync(nativeMessage);
            
            logger.LogWarning($"Bike {assembleBikeMessage.Id} assembled and ready to be shipped!");
            
            //TODO: Bad trick to force the function app shutdown :-)
            //https://github.com/Azure/azure-functions-host/blob/dev/src/WebJobs.Script.WebHost/FileMonitoringService.cs#L164
            await File.WriteAllTextAsync(Path.Combine(executionContext.FunctionAppDirectory, "app_offline.htm"), string.Empty);
        }
    }
}