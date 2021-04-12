using System;
using System.Threading.Tasks;
using FantasticBike.Shared;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FantasticBike.Shipper
{
    public class FunctionEndpoint
    {
        const string EndpointName = "fantastic-bike-shipper";
        readonly NServiceBus.FunctionEndpoint endpoint;
        readonly Random random = new Random();
        public FunctionEndpoint(NServiceBus.FunctionEndpoint endpoint) => this.endpoint = endpoint;

        
        [FunctionName(EndpointName)]
        public Task Run(
            [ServiceBusTrigger(queueName: "%NServiceBus:EndpointName%")] Message message, 
            ILogger logger, 
            ExecutionContext executionContext) =>
            endpoint.Process(message, executionContext, logger);
        
        
        /*
        [FunctionName("ShipBike")]
        public async Task ShipBike(
            [ServiceBusTrigger(queueName: "fantastic-bike-shipper", Connection = "AzureWebJobsServiceBus")] ShipBikeMessage message, 
            ILogger logger)
        {
            logger.LogWarning($"Handling {nameof(ShipBikeMessage)} in {nameof(ShipBike)}.");
            await Task.Delay(TimeSpan.FromSeconds(random.Next(1,5)));
            logger.LogWarning($"Bike {message.Id} shipped to {message.Address}!");
        }
        */
    }
}