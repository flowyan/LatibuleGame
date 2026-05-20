using Engine.Core;
using Engine.Utilities;
using Latibule.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Metadata = Latibule.Core.Metadata;

namespace Latibule;

public static class Program
{
    private static readonly NativeWindowSettings _windowSettings = new()
    {
        ClientSize = new Vector2i(1920, 1080),
        Title = $"{Metadata.GAME_NAME} {Metadata.GAME_VERSION}",
        // Icon = WindowHelper.LoadIcon("Assets/icon.jpg"),
        APIVersion = new Version(4, 6),
        Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,
        Profile = ContextProfile.Core,
        Vsync = VSyncMode.Off,
        NumberOfSamples = 4,
    };

    public static void Main()
    {
        using var game = new LatibuleGame(_windowSettings);
        game.Run();
    }
}