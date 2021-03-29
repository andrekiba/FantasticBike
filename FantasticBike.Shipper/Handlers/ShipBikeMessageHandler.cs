using System;
using System.Threading.Tasks;
using FantasticBike.Shared;
using NServiceBus;
using NServiceBus.Logging;

namespace FantasticBike.Shipper.Handlers
{
    public class ShipBikeMessageHandler : IHandleMessages<ShipBikeMessage>
    {
        static readonly ILog log = LogManager.GetLogger<ShipBikeMessageHandler>();
        static readonly Random random = new Random();
        
        public async Task Handle(ShipBikeMessage message, IMessageHandlerContext context)
        {
            log.Warn($"Handling {nameof(ShipBikeMessage)} in {nameof(ShipBikeMessageHandler)}.");
            await Task.Delay(TimeSpan.FromSeconds(random.Next(1,5)));
            log.Warn($"Bike {message.Id} shipped to {message.Address}!");
        }
    }
}