using Latibule.Core;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Models;

namespace Latibule.Commands;

public class GameObjects : ICommand
{
    public string Name { get; } = "gameobjects";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "gameobjects";

    public Task Execute(string[] args)
    {
        var list = "";
        foreach (var o in LatibuleGame.GameWorld.Objects.Where(o => o.Parent == null))
        {
            var children = $"[{o.Children.Length} {(o.Children.Length == 1 ? "child" : "children")}]";
            list += $"- {o.GetType().Name} at ({o.Position.X},{o.Position.Y},{o.Position.Z}) {(o.Children.Length > 0 ? children : "")}";
            list += "\n";
        }

        DevConsole.CommandLog(list);
        return Task.CompletedTask;
    }
}