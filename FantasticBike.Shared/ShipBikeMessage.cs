using NServiceBus;

namespace FantasticBike.Shared
{
    public class ShipBikeMessage : IMessage
    {
        public string Id { get; }
        public string Address { get; }

        public ShipBikeMessage(string id, string address)
        {
            Id = id;
            Address = address;
        }
    }
}