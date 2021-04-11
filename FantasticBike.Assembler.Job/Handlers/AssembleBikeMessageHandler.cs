using System;
using System.Threading.Tasks;
using Bogus;
using FantasticBike.Shared;
using NServiceBus;
using NServiceBus.Logging;

namespace FantasticBike.Assembler.Job.Handlers
{
    public class AssembleBikeMessageHandler : IHandleMessages<AssembleBikeMessage>
    {
        static readonly ILog log = LogManager.GetLogger<AssembleBikeMessageHandler>();
        static readonly Faker faker = new Faker();
        
        public async Task Handle(AssembleBikeMessage message, IMessageHandlerContext context)
        {
            log.Warn($"Handling {nameof(AssembleBikeMessage)} in {nameof(AssembleBikeMessageHandler)}.");
            await Task.Delay(TimeSpan.FromMinutes(faker.Random.Number(1,10)));
            
            var sendOptions = new SendOptions();
            sendOptions.SetDestination("fantastic-bike-shipper");
            
            await context.Send(new ShipBikeMessage(message.Id, faker.Address.FullAddress()), sendOptions);
            log.Warn($"Bike {message.Id} assembled and ready to be shipped!");
        }
    }
}