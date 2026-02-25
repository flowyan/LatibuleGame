using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Mathematics;

namespace Latibule.Core.Components;

public class TextureComponent : BaseComponent
{
    public Texture[] Textures { get; set; }
    public Vector2 UVScale { get; }
    public float UVRotation { get; }

    public TextureComponent(Texture texture, Vector2? uvscale = null, float uvrotate = 0f)
        : this([texture], uvscale, uvrotate)
    {
    }

    public TextureComponent(Vector2? uvscale = null, float uvrotate = 0f, params Texture[] textures)
        : this(textures, uvscale, uvrotate)
    {
    }

    public TextureComponent(Texture[] textures, Vector2? uvscale = null, float uvrotate = 0f)
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