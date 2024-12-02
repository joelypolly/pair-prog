using Microsoft.AspNetCore.Mvc;
using PairProgrammingApi.DataAccess;
using PairProgrammingApi.Logging;
using PairProgrammingApi.Modules.Background;
using PairProgrammingApi.Modules.Common;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace PairProgrammingApi.Modules.HealthCheck;

[ApiController]
public class HealthCheckController(PairContext dbContext) : ModuleBase(dbContext)
{
    protected static readonly ILogger Log = Serilog.Log.ForContext(typeof(HealthCheckController));

    /// <summary>
    /// Checks the health of the application by adding a new record into the database
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/health", Name=nameof(CheckHealthAsync))]
    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        DbContext.HealthChecks.Add(new()
        {
            CheckSuccessful = true
        });
        Log.Here().Information("Checking health check");
        // returns of the save was successful or not
        return await DbContext.SaveChangesAsync(cancellationToken) == 1;
    }

    [HttpGet("/api/health-with-cache", Name=nameof(CheckHealthWithCacheAsync))]
    public async Task<bool> CheckHealthWithCacheAsync([FromServices] IConnectionMultiplexer mux, CancellationToken cancellationToken = default)
    {
        var redis = mux.GetDatabase();
        const string keyName = $"{nameof(HealthCheckController)}.{nameof(CheckHealthWithCacheAsync)}";
        var health = await redis.StringGetAsync(keyName);
        if (health.IsNull)
        {
            Log.Here().Information("No health check found; adding cache to redis");
            await redis.StringSetAsync(keyName, new(), TimeSpan.FromMinutes(5));
            return false;
        }
        return true;
    }
    
    [HttpGet("/api/health-check-pub-sub", Name=nameof(TriggerRedisPublishing))]
    public async Task TriggerRedisPublishing([FromServices] IConnectionMultiplexer mux, CancellationToken cancellationToken = default)
    {
        var redis = mux.GetDatabase();
        var producerTask = Task.Run(async () =>
        {
            var random = new Random();
            while (!cancellationToken.IsCancellationRequested)
            {
                await redis.StreamAddAsync(RedisSubService.StreamName,
                [
                    new("temp", random.Next(50, 65)), 
                    new("time", DateTimeOffset.Now.ToUnixTimeSeconds())
                ]);
                Log.Here().Information("New event published");
                await Task.Delay(2000, cancellationToken);
            }
        }, cancellationToken);
        await producerTask;
    }
}