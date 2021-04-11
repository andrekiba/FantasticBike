using System;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Bogus;
using FantasticBike.Shared;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FantasticBike.Assembler.Job
{
    public class FunctionEnpoint
    {
        const string EndpointName = "fantastic-bike-assembler";
        readonly NServiceBus.FunctionEndpoint endpoint;
        static readonly Faker faker = new Faker();
        public FunctionEnpoint(NServiceBus.FunctionEndpoint endpoint) => this.endpoint = endpoint;
        
        [FunctionName(EndpointName)]
        public Task Run(
            [ServiceBusTrigger(queueName: "%NServiceBus:EndpointName%")] Message message, 
            ILogger logger, 
            ExecutionContext executionContext) =>
            endpoint.Process(message, executionContext, logger);
        
        /*
        [FunctionName("AssembleBike")]
        public async Task AssembleBike(
            [ServiceBusTrigger("fantastic-bike-assembler", Connection = "AzureWebJobsServiceBus")] AssembleBikeMessage assembleBikeMessage,
            [ServiceBus("fantastic-bike-shipper", Connection = "AzureWebJobsServiceBus")]IAsyncCollector<Message> collector,
            ILogger logger,
            MessageReceiver messageReceiver)
        {
            #region No transaction
            
            logger.LogWarning($"Handling {nameof(AssembleBikeMessage)} in {nameof(AssembleBike)}.");
            await Task.Delay(TimeSpan.FromMinutes(faker.Random.Number(5,10)));

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
            
            #endregion
            
            // #region In transaction
            //
            // using var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
            //
            // logger.LogWarning($"Handling {nameof(AssembleBikeMessage)} in {nameof(AssembleBike)}.");
            // await Task.Delay(TimeSpan.FromMinutes(faker.Random.Number(5,10)));
            //
            // var messageSender = new MessageSender(messageReceiver.ServiceBusConnection, 
            //     entityPath: "fantastic-bike-shipper", viaEntityPath: "fantastic-bike-assembler");
            // var shipBikeMessage = new ShipBikeMessage(assembleBikeMessage.Id, faker.Address.FullAddress());
            // var rowShipBikeMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(shipBikeMessage));
            // var nativeMessage = new Message(rowShipBikeMessage)
            // {
            //     MessageId = Guid.NewGuid().ToString(),
            //     UserProperties =
            //     {
            //         {"NServiceBus.EnclosedMessageTypes", typeof(ShipBikeMessage).FullName}
            //     }
            // };
            // await messageSender.SendAsync(nativeMessage);
            // //TODO: manually set "autoComplete": false in host.json
            // await messageReceiver.CompleteAsync(nativeMessage.SystemProperties.LockToken);
            //
            // logger.LogWarning($"Bike {assembleBikeMessage.Id} assembled and ready to be shipped!");
            //
            // scope.Complete();
            //
            // #endregion            
        }
        */
    }
}