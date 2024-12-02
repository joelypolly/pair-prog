namespace PairProgrammingApi.Logging;

/// <summary>
///     Static class for wrapping logging constants
/// </summary>
public static class LoggingConstants
{
    /// <summary>
    ///     The expression statement for development logging.
    /// </summary>
    /// <returns>An expression statement used for logging in development environments.</returns>
    public const string DevelopmentTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{CorrelationId} {TinyUrl} {SessionId}] {Message:lj} ({Here}){NewLine}{Exception}";

    /// <summary>
    ///     The expression statement for production logging.
    /// </summary>
    /// <returns>An expression statement used for logging in production environments.</returns>
    public const string ProductionTemplate = "[{Level:u3}] [{CorrelationId} {TinyUrl} {SessionId}] {Message:lj} ({Here}){NewLine}{Exception}";
}
