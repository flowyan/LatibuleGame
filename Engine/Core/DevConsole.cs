using System.Reflection;
using Engine.Core.Types;
using ICommand = Engine.Core.Types.ICommand;
using Vector4 = System.Numerics.Vector4;

namespace Engine.Core;

public class DevConsole
{
    public static List<ConsoleMessage> Messages = [];
    public static List<string> CommandHistory = [];
    public static List<ICommand?>? ConsoleCommands = [];

    public static string CurrentCommand = "";
    public static bool IsOpen { get; set; }

    public static void Log(ConsoleMessage message) => Messages.Add(message);

    public static void InfoLog(string message) => Log(new ConsoleMessage(message, ConsoleMessageType.Info));

    public static void ErrorLog(string message) =>
        Log(new ConsoleMessage($"[ERROR] {message}", ConsoleMessageType.Error));

    public static void WarnLog(string message) =>
        Log(new ConsoleMessage($"[WARN] {message}", ConsoleMessageType.Warning));

    public static void CommandLog(string message, Vector4? color = null) =>
        Log(new ConsoleMessage(message, ConsoleMessageType.CommandOutput, color));

    public static void ExecuteCommand(string command)
    {
        try
        {
            command = command.ToLower();
            Logger.LogInfo($"Executing command: {command}", false);
            InfoLog($"] {command}");
            CommandHistory.Add(command);

            // if (!PetrichorEngine.HasDeveloperKey && command.Split(" ")[0] != "sv_cheats")
            // {
            //     Logger.LogError($"Can't use cheat command '{command.Split(" ")[0]}', unless the game has sv_cheats set to 1.");
            //     return;
            // }

            // Support for multiple commands separated by semicolons
            if (command.Contains(';'))
            {
                var commands = command.Split(";");
                foreach (var cmd in commands)
                {
                    var args = cmd.Split(" ");
                    var consoleCommand = ConsoleCommands?.Find(c => c.Name == args[0] || c.Aliases.Contains(args[0]));
                    if (consoleCommand != null) consoleCommand.Execute(args);
                    else Logger.LogError($"Command '{args[0]}' not found");
                }
            }
            else
            {
                var args = command.Split(" ");
                var consoleCommand = ConsoleCommands?.Find(c => c.Name == args[0] || c.Aliases.Contains(args[0]));
                if (consoleCommand != null) consoleCommand.Execute(args);
                else Logger.LogError($"Command '{args[0]}' not found");
            }

            // Clear the command input after execution
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Logger.LogError($"An error occurred while executing command: {e.Message}");
        }

        CurrentCommand = "";
    }

    public static void Initialize()
    {
        // Assign commands for DevConsole
        ConsoleCommands = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(ICommand)))
            .Select(t => Activator.CreateInstance(t) as ICommand)
            .ToList();

        // Load console commands from the calling assembly
        // foreach (var consoleCommand in Assembly.GetCallingAssembly().GetTypes()
        //              .Where(t => t.GetInterfaces().Contains(typeof(ICommand)))
        //              .Select(t => Activator.CreateInstance(t) as ICommand)
        //              .ToList()) ConsoleCommands.Add(consoleCommand);

        Logger.LogInfo($"Loaded {ConsoleCommands.Count} console commands.");
    }
}