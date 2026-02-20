using System.Drawing;
using Latibule.Core.Rendering.Helpers;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Latibule.Core.Rendering;

public class Texture : IDisposable
{
    private int _handle;

    public readonly int Width;
    public readonly int Height;

    public Texture(int width, int height)
    {
        Width = width;
        Height = height;

        _handle = GL.GenTexture();
        GLUtility.CheckError();
        Bind();

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GLUtility.CheckError();

        SetParameters();
    }

    public Texture(string path)
    {
        _handle = GL.GenTexture();
        GLUtility.CheckError();
        Bind();

        StbImage.stbi_set_flip_vertically_on_load(1);

        using (var stream = File.OpenRead(path))
        {
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            Width = image.Width;
            Height = image.Height;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GLUtility.CheckError();
        }

        SetParameters();
    }

    public void SetParameters()
    {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GLUtility.CheckError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GLUtility.CheckError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GLUtility.CheckError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GLUtility.CheckError();

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GLUtility.CheckError();
    }

    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GLUtility.CheckError();

        GL.BindTexture(TextureTarget.Texture2D, _handle);
        GLUtility.CheckError();
    }

    public void Dispose()
    {
        GL.DeleteTexture(_handle);
        GLUtility.CheckError();
        GC.SuppressFinalize(this);
    }

    public unsafe void SetData(Rectangle bounds, byte[] data)
    {
        Bind();
        fixed (byte* ptr = data)
        {
            GL.TexSubImage2D(
                target: TextureTarget.Texture2D,
                level: 0,
                xoffset: bounds.Left,
                yoffset: bounds.Top,
                width: bounds.Width,
                height: bounds.Height,
                format: PixelFormat.Rgba,
                type: PixelType.UnsignedByte,
                pixels: new IntPtr(ptr)
            );
            GLUtility.CheckError();
        }
    }
}