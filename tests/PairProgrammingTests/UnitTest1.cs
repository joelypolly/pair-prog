using PairProgrammingApi.Modules.HealthCheck.Models;
using PairProgrammingTests.DataAccess;
using Xunit;

namespace PairProgrammingTests;

public class Tests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CheckCanCreateData()
    {
        await using var context = DatabaseFixture.CreateContext();
        await context.Database.BeginTransactionAsync();
        context.HealthChecks.Add(new HealthCheckDbModel() { CheckSuccessful = true });
        await context.SaveChangesAsync();
        
        var numberOfHealthChecks = context.HealthChecks.Count();
        Assert.Equal(1, numberOfHealthChecks);
    }
}