// https://gitlab.acidiclight.dev/sociallydistant/sociallydistant/-/blob/master/vendor/FontStashSharp/samples/FontStashSharp.Samples.OpenTK/Platform/Texture2DManager.cs

using System.Drawing;
using FontStashSharp.Interfaces;

namespace Engine.Rendering.Text;

internal class Texture2DManager : ITexture2DManager
{
    public object CreateTexture(int width, int height) => new Texture2D(width, height);

    public Point GetTextureSize(object texture)
    {
        var t = (Texture2D)texture;
        return new Point(t.Width, t.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var t = (Texture2D)texture;
        t.SetData(bounds, data);
    }
}