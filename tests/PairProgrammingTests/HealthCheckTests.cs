using Microsoft.EntityFrameworkCore;
using PairProgrammingApi.Modules.HealthCheck;
using PairProgrammingApi.Modules.HealthCheck.Models;
using PairProgrammingTests.DataAccess;
using Xunit;

namespace PairProgrammingTests;

public class HealthCheckTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CheckCanCreateData()
    {
        await using var context = DatabaseFixture.CreateContext();
        await context.Database.BeginTransactionAsync();
        context.HealthChecks.Add(new (){ CheckSuccessful = true });
        await context.SaveChangesAsync();
        
        var numberOfHealthChecks = context.HealthChecks.Count();
        Assert.Equal(1, numberOfHealthChecks);
    }
    
    [Fact]
    public async Task CheckApiReturnsTrue()
    {
        await using var context = DatabaseFixture.CreateContext();
        await context.Database.BeginTransactionAsync();
        var beforeCaller = DateTimeOffset.UtcNow;
        var controller = new HealthCheckController(context);
        await controller.CheckHealthAsync();
        
        var result = await context.HealthChecks.SingleOrDefaultAsync();
        Assert.NotNull(result);
        // check that it is something we created
        Assert.True(result.CreatedUtc >= beforeCaller && result.CreatedUtc < DateTimeOffset.UtcNow);
    }
}