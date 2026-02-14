using Latibule.Core;
using Latibule.Core.Rendering;
using Latibule.Models;

namespace Latibule.Commands;

public class ReloadWorld : ICommand
{
    public string Name { get; } = "reloadworld";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "reloadworld";

    public Task Execute(string[] args)
    {
        LatibuleGame.GameWorld.Dispose();
        LatibuleGame.GameWorld = new World();
        LatibuleGame.GameWorld = LatibuleGame.CreateWorld();
        LatibuleGame.GameWorld.OnLoad();
        DevConsole.CommandLog("RELOADING WORLD");
        return Task.CompletedTask;
    }
}