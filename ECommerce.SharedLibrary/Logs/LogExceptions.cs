using Serilog;

namespace ECommerce.SharedLibrary.Logs;

public static class LogExceptions
{
    public static void LogExceptionsTo(Exception ex)
    {
        LogToFile(ex.Message);
        LogToConsole(ex.Message);
        LogToDebugger(ex.Message);
    }

    private static void LogToFile(string message) => Log.Information(message);
    private static void LogToConsole(string message) => Log.Warning(message);
    private static void LogToDebugger(string message) => Log.Debug(message);
}