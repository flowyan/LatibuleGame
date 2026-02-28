using JetBrains.Annotations;
using Latibule.Core.Types;

namespace Latibule.Commands;

[UsedImplicitly]
public class Quit : ICommand
{
    public string Name { get; } = "quit";
    public List<string> Aliases { get; } = ["exit", "disconnect", "dc"];
    public string Usage { get; } = "quit";

    public Task Execute(string[] args)
    {
        GameStates.GameWindow.Close();
        return Task.CompletedTask;
    }
}