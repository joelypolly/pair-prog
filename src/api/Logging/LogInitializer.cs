using Destructurama;
using PairProgrammingApi.Setup;
using Serilog;
using Serilog.Events;

namespace PairProgrammingApi.Logging;

public static class LogInitializer
{
     /// <summary>
    ///     Performs the configuration of Serilog for the host.
    /// </summary>
    public static void UseSerilogSetup(this ConfigureHostBuilder host)
    {
        ConfigureSerilog();

        host.UseSerilog();
    }

    /// <summary>
    ///     Performs the actual initialization of Serilog itself.  This can be used for unit testing.
    /// </summary>
    public static void ConfigureSerilog()
    {
        var logConfiguration = new LoggerConfiguration()
            .Destructure.UsingAttributes()
            .Enrich.WithCorrelationId()
            .Enrich.WithCorrelationIdHeader()
            .Enrich.FromLogContext();

        logConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient.Default.LogicalHandler", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient.Default.ClientHandler", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);

        if (RuntimeEnvironment.IsDevelopment)
        {
            logConfiguration.WriteTo
                .Console(outputTemplate: LoggingConstants.DevelopmentTemplate)
                .MinimumLevel.Debug();
        }
        else
        {
            logConfiguration.WriteTo
                .Console(outputTemplate: LoggingConstants.ProductionTemplate)
                .MinimumLevel.Information();
        }

        Log.Logger = logConfiguration.CreateLogger();
    }
}