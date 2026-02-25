using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Latibule.Core.Rendering.Helpers;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;


namespace Latibule.Core.Rendering;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class TextureOptions
{
    public TextureWrapMode WrapMode = TextureWrapMode.Repeat;
    public TextureMinFilter MinFilter = TextureMinFilter.Nearest;
    public TextureMagFilter MagFilter = TextureMagFilter.Nearest;
}

public class Texture : IDisposable
{
    private int _handle;

    public readonly int Width;
    public readonly int Height;
    public bool HasTransparency { get; private set; }

    public Texture(int width, int height, TextureOptions? options = null)
    {
        options ??= new TextureOptions();
        Width = width;
        Height = height;

        _handle = GL.GenTexture();
        GLUtility.CheckError();
        Bind();

        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GLUtility.CheckError();

        SetParameters(options);
    }

    public Texture(string path, TextureOptions? options = null)
    {
        options ??= new TextureOptions();
        _handle = GL.GenTexture();
        GLUtility.CheckError();
        Bind();

        StbImage.stbi_set_flip_vertically_on_load(1);

        using (var stream = File.OpenRead(path))
        {
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            HasTransparency = DetectTransparency(image.Data);
            Width = image.Width;
            Height = image.Height;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GLUtility.CheckError();
        }

        SetParameters(options);
    }

    public void SetParameters(TextureOptions options)
    {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)options.WrapMode);
        GLUtility.CheckError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)options.WrapMode);
        GLUtility.CheckError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)options.MinFilter);
        GLUtility.CheckError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)options.MagFilter);
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

    private static bool DetectTransparency(byte[] rgba)
    {
        // RGBA = 4 bytes per pixel, alpha at index 3
        for (int i = 3; i < rgba.Length; i += 4)
            if (rgba[i] != 255)
                return true;
        return false;
    }
}