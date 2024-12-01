using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using PairProgrammingApi.Logging;
using PairProgrammingApi.Modules.HealthCheck.Models;

namespace PairProgrammingApi.DataAccess;

public class PairContext : DbContext
{
    /// <summary>
    /// Creates the db context from the context options
    /// </summary>
    /// <param name="options"></param>
    public PairContext(DbContextOptions<PairContext> options) : base(options)
    {
    }

    public DbSet<HealthCheckDbModel> HealthChecks { get; set; }


    #region Overrides
    /// <summary>
    /// Overloads the default SaveChanges()
    /// </summary>
    /// <returns></returns>
    public int SaveChanges(
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var context = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}:{callerMemberName}@{sourceLineNumber}";
        UpdateEntityBaseInfo(context);
        return base.SaveChanges();
    }

    /// <summary>
    /// Overloads the default SaveChangesAsync()
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="callerMemberName"></param>
    /// <param name="sourceFilePath"></param>
    /// <param name="sourceLineNumber"></param>
    /// <returns></returns>
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var context = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}:{callerMemberName}@{sourceLineNumber}";
        UpdateEntityBaseInfo(context);
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update the timestamps and context
    /// </summary>
    /// <param name="context">The context string of the call</param>
    private void UpdateEntityBaseInfo(string context = "UNKNOWN")
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e is { Entity: EntityBase, State: EntityState.Added or EntityState.Modified });

        foreach (var entry in entries)
        {
            var entity = (EntityBase)entry.Entity;
            switch (entry.State)
            {
                case EntityState.Modified:
                    entity.LastUpdatedUtc = DateTime.UtcNow;
                    entity.LastUpdateContext = context;
                    break;
                case EntityState.Added:
                    entity.CreatedUtc = DateTime.UtcNow;
                    entity.CreatedContext = context;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                default:
                    break;
            }
        }
    }
    #endregion
}