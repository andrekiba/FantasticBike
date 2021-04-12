using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FantasticBike.Shared;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FantasticBike.Assembler.WorkerJob
{
    public class Worker : BackgroundService
    {
        readonly ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder;
        readonly IConfiguration configuration;
        readonly ILogger<Worker> logger;
        readonly QueueClient assemblerQueue;
        readonly QueueClient shipperQueue;
        readonly Faker faker = new();
        readonly IHostApplicationLifetime appLifetime;
        
        public Worker(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, IConfiguration configuration, 
            IHostApplicationLifetime appLifetime,
            ILogger<Worker> logger)
        {
            this.serviceBusConnectionStringBuilder = serviceBusConnectionStringBuilder;
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            assemblerQueue = new QueueClient(serviceBusConnectionStringBuilder.GetNamespaceConnectionString(), "fantastic-bike-assembler");
            shipperQueue = new QueueClient(serviceBusConnectionStringBuilder.GetNamespaceConnectionString(), "fantastic-bike-shipper");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var messageHandlerOptions = new MessageHandlerOptions(HandleReceivedException)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false,
                MaxAutoRenewDuration = TimeSpan.FromHours(1)
            };
            assemblerQueue.RegisterMessageHandler(HandleMessage, messageHandlerOptions);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            
            await assemblerQueue.CloseAsync();
        }
        async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var rawMessageBody = Encoding.UTF8.GetString(message.Body);
            logger.LogInformation("Received message {MessageId} with body {MessageBody}", message.MessageId, rawMessageBody);

            var assembleBikeMessage = JsonConvert.DeserializeObject<AssembleBikeMessage>(rawMessageBody);
            if (assembleBikeMessage != null)
            {
                await Task.Delay(TimeSpan.FromMinutes(faker.Random.Number(1,5)), cancellationToken);

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
            
                await shipperQueue.SendAsync(nativeMessage);
            
                logger.LogWarning($"Bike {assembleBikeMessage.Id} assembled and ready to be shipped!");
            }
            else
                logger.LogError("Unable to deserialize to message contract {ContractName} for message {MessageBody}", typeof(AssembleBikeMessage), rawMessageBody);
            
            logger.LogInformation("Message {MessageId} processed", message.MessageId);

            await assemblerQueue.CompleteAsync(message.SystemProperties.LockToken);
            
            appLifetime.StopApplication();
        }
        Task HandleReceivedException(ExceptionReceivedEventArgs exceptionEvent)
        {
            logger.LogError(exceptionEvent.Exception, "Unable to process message");
            return Task.CompletedTask;
        }
    }
}