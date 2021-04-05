using System.Collections.Generic;
using System.Runtime.Serialization;
using NServiceBus;

namespace FantasticBike.Shared
{
    public class AssembleBikeMessage : IMessage
    {
        public string Id { get; set; }
        public int Price { get; set; }
        public string Model { get; set; }
        public List<BikePart> Parts { get; set; }

        public AssembleBikeMessage()
        {
            
        }
        public AssembleBikeMessage(string id, int price, string model, List<BikePart> parts)
        {
            Id = id;
            Price = price;
            Model = model;
            Parts = parts;
        }
    }
}