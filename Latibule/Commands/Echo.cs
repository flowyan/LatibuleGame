using JetBrains.Annotations;
using Latibule.Core;
using Latibule.Models;

namespace Latibule.Commands;

[UsedImplicitly]
public class Echo : ICommand
{
    public string Name { get; } = "echo";
    public List<string> Aliases { get; } = ["say", "repeat"];
    public string Usage { get; } = "echo <message>";

    public Task Execute(string[] args)
    {
        if (args.Length == 1)
        {
            DevConsole.CommandLog("[ERROR] echo: missing argument");
            return Task.CompletedTask;
        }

        var message = string.Join(" ", args[1..]);
        DevConsole.CommandLog(message);
        return Task.CompletedTask;
    }
}