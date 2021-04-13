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
    public class AssemblerWorker : BackgroundService
    {
        readonly ServiceBusConnectionStringBuilder asbConnectionBuilder;
        readonly IConfiguration configuration;
        readonly ILogger<AssemblerWorker> logger;
        readonly QueueClient assemblerQueue;
        readonly QueueClient shipperQueue;
        readonly Faker faker = new();
        readonly IHostApplicationLifetime appLifetime;
        bool workCompleted;
        
        public AssemblerWorker(ServiceBusConnectionStringBuilder asbConnectionBuilder, IConfiguration configuration, 
            IHostApplicationLifetime appLifetime,
            ILogger<AssemblerWorker> logger)
        {
            this.asbConnectionBuilder = asbConnectionBuilder;
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            assemblerQueue = new QueueClient(asbConnectionBuilder.GetNamespaceConnectionString(), "fantastic-bike-assembler");
            shipperQueue = new QueueClient(asbConnectionBuilder.GetNamespaceConnectionString(), "fantastic-bike-shipper");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            try
            {
                var messageHandlerOptions = new MessageHandlerOptions(HandleReceivedException)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false,
                    MaxAutoRenewDuration = TimeSpan.FromHours(1)
                };
                assemblerQueue.RegisterMessageHandler(HandleMessage, messageHandlerOptions);

                while (!workCompleted && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Fatal error");
            }
            finally
            {
                await assemblerQueue.CloseAsync();
                appLifetime.StopApplication();
            }
        }, stoppingToken);
        
        async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var rawMessageBody = Encoding.UTF8.GetString(message.Body);
            logger.LogInformation("Received message {MessageId} with body {MessageBody}", message.MessageId, rawMessageBody);

            var assembleBikeMessage = JsonConvert.DeserializeObject<AssembleBikeMessage>(rawMessageBody);
            if (assembleBikeMessage != null)
            {
                await Task.Delay(TimeSpan.Parse(configuration.GetValue<string>("FakeWorkDuration")), cancellationToken);

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
            
            await assemblerQueue.CompleteAsync(message.SystemProperties.LockToken);
            logger.LogInformation("Message {MessageId} processed", message.MessageId);

            workCompleted = true;
        }
        Task HandleReceivedException(ExceptionReceivedEventArgs exceptionEvent)
        {
            logger.LogError(exceptionEvent.Exception, "Unable to process message");
            return Task.CompletedTask;
        }
    }
}