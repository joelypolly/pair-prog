using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PairProgrammingApi.DataAccess;

namespace PairProgrammingTests.DataAccess;

public class DatabaseFixture
{
    private static readonly string _connectionString =
        "server=127.0.0.1;port=6543;database=unit_test;user id=postgres;password=postgres;include error detail=true";

    private static readonly object _sync = new();
    private static bool _initialized;

    /// <summary>
    ///     Constructor which initializes the database for testing each run.
    /// </summary>
    public DatabaseFixture()
    {
        // Use simple double lock-check mechanism.
        if (_initialized)
        {
            return;
        }

        lock (_sync)
        {
            if (_initialized)
            {
                return;
            }

            using var context = CreateContext();
            // Drop and create the database for each test run.
            Console.WriteLine("Creating test database...");
            context.Database.EnsureDeleted();
            context.Database.Migrate();
            Console.WriteLine("Dropped and created test database.");

            _initialized = true;
        }
    }

    // Set up the JSONB conversion
    private static NpgsqlDataSource? _dataSourceBuilder;

    private static NpgsqlDataSource DataSource
    {
        get
        {
            if (_dataSourceBuilder == null)
            {
                var npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
                npgsqlDataSourceBuilder.EnableDynamicJson();
                _dataSourceBuilder = npgsqlDataSourceBuilder.Build();
            }

            return _dataSourceBuilder;
        }
    }

    /// <summary>
    ///     Creates a new context for test purposes.
    /// </summary>
    /// <returns>A fully configured context.</returns>
    public static PairContext CreateContext(bool logSql = false)
    {
        var config = new DbContextOptionsBuilder<PairContext>()
            // Uncomment the next line to output the queries.
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseNpgsql(
                DataSource, pg => { pg.UseAdminDatabase("postgres"); })
            // Postgres does not like capital letters so switch to snake_case
            .UseSnakeCaseNamingConvention()
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        return new(config.Options);
    }
}