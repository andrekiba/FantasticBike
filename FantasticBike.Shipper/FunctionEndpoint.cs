using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FantasticBike.Shipper
{
    public class FunctionEndpoint
    {
        const string EndpointName = "bike-shipper";

        readonly NServiceBus.FunctionEndpoint endpoint;
        public FunctionEndpoint(NServiceBus.FunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName(EndpointName)]
        public Task Run(
            [ServiceBusTrigger(queueName: "%NServiceBus:EndpointName%")] Message message, 
            ILogger logger, 
            ExecutionContext executionContext) =>
            endpoint.Process(message, executionContext, logger);
    }
}