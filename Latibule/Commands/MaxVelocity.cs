using System.Numerics;
using Latibule.Core;
using Latibule.Models;
using JetBrains.Annotations;

namespace Latibule.Commands;

[UsedImplicitly]
public class MaxVelocity : ICommand
{
    public string Name { get; } = "max_velocity";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "max_velocity <float>";

    public Task Execute(string[] args)
    {
        if (args.Length == 1)
        {
            DevConsole.CommandLog("[ERROR] max_velocity: missing argument");
            return Task.CompletedTask;
        }

        if (!float.TryParse(args[1], out var floatValue))
        {
            DevConsole.CommandLog("[ERROR] fov: invalid float", new Vector4(1, 0, 0, 1));
            return Task.CompletedTask;
        }

        LatibuleGame.Player.MaxVelocity = floatValue;
        DevConsole.CommandLog($"Player's 'MaxVelocity' Set to {args[1]}");
        return Task.CompletedTask;
    }
}