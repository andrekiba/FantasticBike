using FantasticBike.Shipper;
using FantasticBike.Shipper.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NServiceBus;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FantasticBike.Shipper
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;

            builder.Services.ConfigureLogger(config);
            
            NServiceBusExtensions.CreateTopology(config, 
                    auditQueue: config["NServiceBus:AuditQueue"], 
                    errorQueue: config["NServiceBus:ErrorQueue"])
                .GetAwaiter().GetResult();
            
            builder.UseNServiceBus(() => NServiceBusExtensions.BuildEndpointConfiguration(config));
        }
    }
}