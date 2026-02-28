using Latibule.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp.PixelFormats;
using Image = OpenTK.Windowing.Common.Input.Image;

namespace Latibule;

public static class Program
{
    private static readonly NativeWindowSettings _windowSettings = new()
    {
        ClientSize = new Vector2i(1920, 1080),
        Title = $"{Metadata.GAME_NAME} {Metadata.GAME_VERSION}",
        Icon = LoadIcon("Assets/icon.jpg"),
        APIVersion = new Version(4, 6),
        Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,
        Profile = ContextProfile.Core,
        Vsync = VSyncMode.Off,
        NumberOfSamples = 4,
    };

    private static WindowIcon LoadIcon(string path)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

        var pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);

        return new WindowIcon(
            new Image(
                image.Width,
                image.Height,
                pixels
            )
        );
    }

    public static void Main()
    {
        using var game = new LatibuleGame(_windowSettings);
        game.Run();
    }
}