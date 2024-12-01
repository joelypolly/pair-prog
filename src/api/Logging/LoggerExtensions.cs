using System.Runtime.CompilerServices;

namespace PairProgrammingApi.Logging;

public static class LoggerExtensions
{
    /// <summary>
    ///     At the top of the class:
    ///     private static Serilog.ILogger Log => Serilog.Log.ForContext{ClassNameHere}();
    ///     Then below:
    ///     Log.Here().Information("...")
    /// </summary>
    public static Serilog.ILogger Here(
        this Serilog.ILogger logger,
        [CallerMemberName]
        string memberName = "",
        [CallerFilePath]
        string sourceFilePath = "",
        [CallerLineNumber]
        int sourceLineNumber = 0
    )
    {
        var srcFile = Path.GetFileName(sourceFilePath);
        var here = $" {srcFile}:{memberName}@{sourceLineNumber}";

        return logger
            .ForContext("Here", here)
            .ForContext("MemberName", memberName)
            .ForContext("FilePath", sourceFilePath)
            .ForContext("LineNumber", sourceLineNumber);
    }
}