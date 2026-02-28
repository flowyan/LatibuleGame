using JetBrains.Annotations;
using Latibule.Core;
using Latibule.Core.Types;

namespace Latibule.Commands;

[UsedImplicitly]
public class ListGameObjects : ICommand
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
            list += $"- ({o.GetHashCode()}) {o.GetType().Name} at ({o.Transform.Position.X},{o.Transform.Position.Y},{o.Transform.Position.Z}) {(o.Children.Length > 0 ? children : "")}";
            list += "\n";
        }

        DevConsole.CommandLog(list);
        return Task.CompletedTask;
    }
}