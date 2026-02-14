using System.Runtime.CompilerServices;
using Latibule.Models;

namespace Latibule.Core;

public static class Logger
{
    public static void LogInfo(string message, bool logToDevConsole = true, [CallerFilePath] string caller = "") =>
        Log(message, ConsoleMessageType.Info, logToDevConsole, caller);

    public static void LogWarning(string message, bool logToDevConsole = true, [CallerFilePath] string caller = "") =>
        Log(message, ConsoleMessageType.Warning, logToDevConsole, caller);

    public static void LogError(string message, bool logToDevConsole = true, [CallerFilePath] string caller = "") =>
        Log(message, ConsoleMessageType.Error, logToDevConsole, caller);

    public static void LogDebug(string message, bool logToDevConsole = true, [CallerFilePath] string caller = "") =>
        Log(message, ConsoleMessageType.Debug, logToDevConsole, caller);

    private static void Log(string message, ConsoleMessageType messageType, bool logToDevConsole,
        [CallerFilePath] string caller = "")
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var logPrefix = $"{messageType + "/" + Path.GetFileNameWithoutExtension(caller)}";
        var logMessage = $"[{time}] [{logPrefix}]: {message}";

        switch (messageType)
        {
            case ConsoleMessageType.Info:
                // Console.ForegroundColor = ConsoleColor.White;
                break;
            case ConsoleMessageType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case ConsoleMessageType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case ConsoleMessageType.Debug:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
        }

        if (logToDevConsole) DevConsole.Log(new ConsoleMessage(logMessage, messageType));
        Console.WriteLine(logMessage);
        Console.ResetColor();
    }
}