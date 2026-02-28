using JetBrains.Annotations;
using Latibule.Core;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Core.Types;

namespace Latibule.Commands;

[UsedImplicitly]
public class ListLights : ICommand
{
    public string Name { get; } = "lights";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "lights";

    public Task Execute(string[] args)
    {
        var list = "";
        foreach (var l in LatibuleGame.GameWorld.Lights)
        {
            if (l is null) continue;
            list += $"- Light at ({l.Position.X},{l.Position.Y},{l.Position.Z}) (r{l.Color.X} g{l.Color.Y} b{l.Color.Z})";
            list += "\n";
        }

        DevConsole.CommandLog(list);
        return Task.CompletedTask;
    }
}