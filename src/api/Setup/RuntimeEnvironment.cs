namespace PairProgrammingApi.Setup;

public static class RuntimeEnvironment
{
    /// <summary>
    ///     Returns true when the runtime is in a unit test or CI.
    /// </summary>
    public static bool IsAutomation
    {
        get
        {
            var ci = Environment.GetEnvironmentVariable("CI");
            var ga = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");

            return ci?.ToLowerInvariant() == "true" || ga?.ToLowerInvariant() == "true";
        }
    }
    
    /// <summary>
    ///     Convenience method which determines if the current runtime is a development environment.  Allows abstraction and
    ///     specification of multiple rules for distinguishing development environments.
    /// </summary>
    public static bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                        == Environments.Development;
    
    /// <summary>
    ///     Returns true when the commandline includes the parameter <c>logsql</c>.  This will force EF
    ///     to dump the SQL to the commandline which can be very noisy so be aware when you use this option.
    /// </summary>
    public static bool LogSql =>
        Environment.GetCommandLineArgs().Any(i=> i.Equals("logsql", StringComparison.OrdinalIgnoreCase))
        && IsDevelopment;
}