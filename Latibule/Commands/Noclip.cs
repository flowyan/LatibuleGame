using System.Numerics;
using JetBrains.Annotations;
using Latibule.Core;
using Latibule.Core.Gameplay;
using Latibule.Core.Types;

namespace Latibule.Commands;

[UsedImplicitly]
public class Noclip : ICommand
{
    public string Name { get; } = "noclip";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "noclip";

    public Task Execute(string[] args)
    {
        LatibuleGame.Player.ToggleNoclip();
        DevConsole.CommandLog($"Noclip set to {LatibuleGame.Player.IsNoclip.ToString()}");
        return Task.CompletedTask;
    }
}