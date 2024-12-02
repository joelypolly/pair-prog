using PairProgrammingApi.Logging;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace PairProgrammingApi.Modules.Background;

public class RedisSubService(IConnectionMultiplexer mux): BackgroundService
{
    protected static readonly ILogger Log = Serilog.Log.ForContext(typeof(RedisSubService));

    public const string StreamName = "telemetry";
    public const string GroupName = "avg";
    
    private readonly IDatabase _database = mux.GetDatabase(); 
    
    /// <summary>
    /// Basically the main loop 
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // on first run create the stream if it doesn't exist
        if (!await _database.KeyExistsAsync(StreamName) ||
            (await _database.StreamGroupInfoAsync(StreamName)).All(x => x.Name != GroupName))
        {
            await _database.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0", true);
        }

        double count = default;
        double total = default;
        
        var consumerGroupReadTask = Task.Run(async () =>
        {
            var id = string.Empty;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    await _database.StreamAcknowledgeAsync(StreamName, GroupName, id);
                    id = string.Empty;
                }
                var result = await _database.StreamReadGroupAsync(StreamName, GroupName, "avg-1", ">", 1);
                if (result.Length != 0)
                {
                    id = result.First().Id!;
                    count++;
                    var dict = ParseResult(result.First());
                    total += double.Parse(dict["temp"]);
                    Log.Here().Information($"Group read result: temp: {dict["temp"]}, time: {dict["time"]}, current average: {total/count:00.00}");
                }
                
                await Task.Delay(1000, stoppingToken);
            }
        }, stoppingToken);

        // keep both tasks running until the cancellation token is set
        Task.WaitAll([consumerGroupReadTask], stoppingToken);
    }
    
    private static Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
}