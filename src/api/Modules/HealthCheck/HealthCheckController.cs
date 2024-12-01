using Microsoft.AspNetCore.Mvc;
using PairProgrammingApi.DataAccess;
using PairProgrammingApi.Logging;
using PairProgrammingApi.Modules.Common;
using ILogger = Serilog.ILogger;

namespace PairProgrammingApi.Modules.HealthCheck;

[ApiController]
public class HealthCheckController(PairContext dbContext) : ModuleBase(dbContext)
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(HealthCheckController));

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
}