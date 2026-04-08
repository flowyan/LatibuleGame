using Engine.Core;
using Engine.Core.Types;
using JetBrains.Annotations;

namespace Engine.Commands;

[UsedImplicitly]
public class Quit : ICommand
{
    public string Name { get; } = "quit";
    public List<string> Aliases { get; } = ["exit", "disconnect", "dc"];
    public string Usage { get; } = "quit";

    public Task Execute(string[] args)
    {
        EngineStates.GameWindow.Close();
        return Task.CompletedTask;
    }
}