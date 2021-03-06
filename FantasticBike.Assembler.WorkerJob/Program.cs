using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace FantasticBike.Assembler.WorkerJob
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.secret.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => config.AddConfiguration(configuration))
                .ConfigureLogging((hostBuilderContext, loggingBuilder) =>
                {
                    var logger = new LoggerConfiguration()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                        .MinimumLevel.Error()
                        .CreateLogger();

                    loggingBuilder.AddSerilog(logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient(s =>
                    {
                        var namespaceConnectionString = configuration.GetValue<string>("ASBConnectionString");
                        return new ServiceBusConnectionStringBuilder(namespaceConnectionString);
                    });
                    services.AddHostedService<AssemblerWorker>();
                });
        }
    }
}