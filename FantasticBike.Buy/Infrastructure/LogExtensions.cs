using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace FantasticBike.Buy.Infrastructure
{
    public static class LogExtensions
    {
        public static void ConfigureLogger(this IServiceCollection services, IConfiguration config)
        {
            var logger = new LoggerConfiguration()
#if DEBUG
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .MinimumLevel.Error()
#else
            .WriteTo.AzureTableStorage(config.GetValue<string>("AzureWebJobsStorage"), storageTableName: $"{nameof(Buy)}Log")
            .MinimumLevel.Error()
#endif
                .CreateLogger();

            services.AddLogging(lb => lb.AddSerilog(logger));
        }
    }
}