using System.Collections.Generic;
using NServiceBus;

namespace FantasticBike.Shared
{
    public class AssembleBikeMessage : IMessage
    {
        public string Id { get; }
        public int Price { get; }
        public string Model { get; }
        public List<BikePart> Parts { get; }

        public AssembleBikeMessage(string id, int price, string model, List<BikePart> parts)
        {
            Id = id;
            Price = price;
            Model = model;
            Parts = parts;
        }
    }
}