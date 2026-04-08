using Engine;
using Engine.Core.Types;
using Engine.Rendering;
using JetBrains.Annotations;
using static Engine.Core.Logger;

namespace Latibule.Commands;

[UsedImplicitly]
public class ReloadWorld : ICommand
{
    public string Name { get; } = "reloadworld";
    public List<string> Aliases { get; } = ["reload"];
    public string Usage { get; } = "reloadworld";

    public Task Execute(string[] args)
    {
        var player = LatibuleGame.Player;
        var camera = LatibuleEngine.Camera;
        var noclip = LatibuleGame.Player.IsNoclip;
        LatibuleEngine.Map = new GameMap();
        LatibuleEngine.Map = TestingMap.Create();
        LatibuleEngine.Map.OnLoad();
        LogWarning("RECREATING WORLD");
        LatibuleGame.Player.Transform = player.Transform;
        LatibuleEngine.Camera.Direction = camera.Direction;
        LatibuleEngine.Camera.Position = camera.Position;
        LatibuleEngine.Camera.View = camera.View;
        LatibuleGame.Player.IsNoclip = noclip;
        return Task.CompletedTask;
    }
}