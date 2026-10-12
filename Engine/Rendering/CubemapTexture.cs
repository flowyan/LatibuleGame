// https://learnopengl.com/Advanced-OpenGL/Cubemaps

using Engine.Rendering.Helpers;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Engine.Rendering;

// GL_TEXTURE_CUBE_MAP_POSITIVE_X	Right
// GL_TEXTURE_CUBE_MAP_NEGATIVE_X	Left
// GL_TEXTURE_CUBE_MAP_POSITIVE_Y	Top
// GL_TEXTURE_CUBE_MAP_NEGATIVE_Y	Bottom
// GL_TEXTURE_CUBE_MAP_POSITIVE_Z	Back
// GL_TEXTURE_CUBE_MAP_NEGATIVE_Z	Front

public class CubemapTexture : IDisposable
{
    private int _handle;

    public readonly int Width;
    public readonly int Height;

    /// <summary>
    /// Creates a cubemap texture from 6 image paths.
    /// </summary>
    /// <param name="paths">right, left, top, bottom, front, back</param>
    public CubemapTexture(string[] paths)
    {
        _handle = GL.GenTexture();
        GLUtility.CheckError();
        Bind();

        for (var i = 0; i < paths.Length; i++)
        {
            StbImage.stbi_set_flip_vertically_on_load(0);

            var path = paths[i];
            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            Width = image.Width;
            Height = image.Height;
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba8, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GLUtility.CheckError();
        }

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
    }

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(textureUnit);
        GLUtility.CheckError();

        GL.BindTexture(TextureTarget.TextureCubeMap, _handle);
        GLUtility.CheckError();
    }

    public void Dispose()
    {
        GL.DeleteTexture(_handle);
        GLUtility.CheckError();
        GC.SuppressFinalize(this);
    }
}