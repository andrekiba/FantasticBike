using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FantasticBike.Assembler.Job
{
    public class FunctionEnpoint
    {
        const string EndpointName = "fantastic-bike-assembler";
        readonly NServiceBus.FunctionEndpoint endpoint;
        public FunctionEnpoint(NServiceBus.FunctionEndpoint endpoint) => this.endpoint = endpoint;
        
        [FunctionName(EndpointName)]
        public Task Run(
            [ServiceBusTrigger(queueName: "%NServiceBus:EndpointName%")] Message message, 
            ILogger logger, 
            ExecutionContext executionContext) =>
            endpoint.Process(message, executionContext, logger);
    }
}