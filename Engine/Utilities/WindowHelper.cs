using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp.PixelFormats;
using Image = OpenTK.Windowing.Common.Input.Image;

namespace Engine.Utilities;

public static class WindowHelper
{
    public static WindowIcon LoadIcon(string path)
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
}