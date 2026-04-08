using System.Numerics;
using Engine.Core;
using Engine.Core.Types;
using JetBrains.Annotations;

namespace Engine.Commands;

[UsedImplicitly]
public class Fov : ICommand
{
    public string Name { get; } = "fov";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "fov <float>";

    public Task Execute(string[] args)
    {
        if (args.Length == 1)
        {
            DevConsole.CommandLog("[ERROR] fov: missing argument");
            return Task.CompletedTask;
        }

        if (!float.TryParse(args[1], out var floatValue))
        {
            DevConsole.CommandLog("[ERROR] fov: invalid float", new Vector4(1, 0, 0, 1));
            return Task.CompletedTask;
        }

        // GameOptions.FieldOfView = floatValue;
        Camera.Fov = floatValue;
        LatibuleEngine.Camera.UpdateProjectionMatrix();
        DevConsole.CommandLog($"FOV Set to {args[1]}");
        return Task.CompletedTask;
    }
}