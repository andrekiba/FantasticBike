using NServiceBus;

namespace FantasticBike.Shared
{
    public class Bike : IMessage
    {
        public string Id { get; }
        public int Cost { get; }
        public string Model { get; }

        public Bike(string id, int cost, string model)
        {
            Id = id;
            Cost = cost;
            Model = model;
        }
    }
}