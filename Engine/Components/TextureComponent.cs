using Engine.Core.ECS;
using Engine.Rendering;
using OpenTK.Mathematics;

namespace Engine.Components;

public class TextureComponent : BaseComponent
{
    public Texture2D[] Textures { get; set; }
    public Vector2 UVScale { get; }
    public float UVRotation { get; }

    public TextureComponent(Texture2D texture, Vector2? uvscale = null, float uvrotate = 0f)
        : this([texture], uvscale, uvrotate)
    {
    }

    public TextureComponent(Vector2? uvscale = null, float uvrotate = 0f, params Texture2D[] textures)
        : this(textures, uvscale, uvrotate)
    {
    }

    public TextureComponent(Texture2D[] textures, Vector2? uvscale = null, float uvrotate = 0f)
    {
        if (textures is null || textures.Length == 0)
            throw new ArgumentException("At least one texture is required.", nameof(textures));

        if (Array.Exists(textures, t => t is null))
            throw new ArgumentException("Textures cannot contain null entries.", nameof(textures));

        Textures = textures;
        UVScale = uvscale ?? Vector2.One;
        UVRotation = uvrotate;
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var tex in Textures)
            tex.Dispose();
        GC.SuppressFinalize(this);
    }
}