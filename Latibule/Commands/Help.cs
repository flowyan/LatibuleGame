using JetBrains.Annotations;
using Latibule.Core;
using Latibule.Core.Types;

namespace Latibule.Commands;

[UsedImplicitly]
public class Help : ICommand
{
    public string Name { get; } = "help";
    public List<string> Aliases { get; } = ["?"];
    public string Usage { get; } = "help | help <command>";

    public Task Execute(string[] args)
    {
        if (args.Length == 1)
        {
            DevConsole.CommandLog("Available commands:");
            foreach (var command in DevConsole.ConsoleCommands)
            {
                DevConsole.CommandLog($"> {command.Name}");
            }
        }

        if (args.Length == 2)
        {
            var command = DevConsole.ConsoleCommands.Find(c => c.Name == args[1] || c.Aliases.Contains(args[0]));
            if (command == null)
            {
                DevConsole.CommandLog($"[ERROR] help: command '{args[1]}' not found");
                return Task.CompletedTask;
            }

            var aliases = "";
            if (command.Aliases.Count > 0) aliases = $" ({string.Join(",", command.Aliases)})";

            DevConsole.CommandLog($"{command.Name} {aliases} : {command.Usage}");
        }

        if (args.Length > 2) DevConsole.ErrorLog("help: too many arguments");

        return Task.CompletedTask;
    }
}