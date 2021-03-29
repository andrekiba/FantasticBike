using System.Collections.Generic;
using NServiceBus;

namespace FantasticBike.Shared
{
    public class BikeParts : IMessage
    {
        public List<BikePart> Parts { get; }

        public BikeParts(List<BikePart> parts)
        {
            Parts = parts;
        }
    }
}