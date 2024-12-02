using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using PairProgrammingApi.DataAccess;
using PairProgrammingApi.Logging;
using PairProgrammingApi.Modules.Background;
using StackExchange.Redis;

namespace PairProgrammingApi.Setup;

public static class SetupServicesExtension
{
    private static Serilog.ILogger Log = Serilog.Log.ForContext(typeof(SetupServicesExtension));

    public static IServiceCollection AddPairServices(this IServiceCollection services, string connectionString)
    {
        // JSON serialization options; enums as strings.
        // HTTP output
        services.ConfigureHttpJsonOptions(
            json =>
            {
                json.SerializerOptions.PropertyNameCaseInsensitive = true;
                json.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                json.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        // JSON serialization options; enums as strings.
        // OpenAPI
        services.Configure<JsonOptions>(
            json =>
                json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Set up CORS
        var origins = new[]
        {
            "http://localhost:5266"
        };

        services
            .AddCors(
                config => config.AddDefaultPolicy(
                    policy => policy
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins(origins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod()));

        // Setup Database with dynamic JSONB support
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();
        services.AddDbContext<PairContext>(
            options =>
            {
                var isDevelopment = !RuntimeEnvironment.IsDevelopment;
                options.UseNpgsql(
                        dataSource, pg => { pg.UseAdminDatabase("postgres"); })
                    // Postgres does not like capital letters so switch to snake_case
                    .UseSnakeCaseNamingConvention()
                    .EnableDetailedErrors(isDevelopment)
                    .EnableSensitiveDataLogging(isDevelopment)
                    .ConfigureWarnings(
                        w =>
                        {
                            if (isDevelopment)
                            {
                                w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
                            }

                            w.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning);
                        });

                if (RuntimeEnvironment.LogSql)
                {
                    // This will dump SQL to the command line; note that this can be noisy!
                    options.UseLoggerFactory(LoggerFactory.Create(o => o.AddConsole()));
                }
            });
        
        // Setup Redis
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
        services.AddHttpClient();
        // Add redis based pub/sub
        services.AddHostedService<RedisSubService>();
        
        Log.Here().Information("Configured services");
        return services;
    }
}