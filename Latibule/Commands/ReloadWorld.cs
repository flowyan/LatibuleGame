using JetBrains.Annotations;
using Latibule.Core.Rendering;
using Latibule.Core.Types;
using static Latibule.Core.Logger;

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
        var noclip = LatibuleGame.Player.IsNoclip;
        LatibuleGame.GameWorld = new World();
        LatibuleGame.GameWorld = TestWorld.Create();
        LatibuleGame.GameWorld.OnLoad();
        LogWarning("RECREATING WORLD");
        LatibuleGame.Player.Transform = player.Transform;
        LatibuleGame.Player.Camera.Direction = player.Camera.Direction;
        LatibuleGame.Player.Camera.Position = player.Camera.Position;
        LatibuleGame.Player.Camera.View = player.Camera.View;
        LatibuleGame.Player.IsNoclip = noclip;
        return Task.CompletedTask;
    }
}