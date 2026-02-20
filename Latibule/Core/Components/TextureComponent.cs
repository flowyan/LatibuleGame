using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Mathematics;

namespace Latibule.Core.Components;

public class TextureComponent(Texture texture, Vector2? uvscale = null, float uvrotate = 0f) : BaseComponent
{
    public Texture? Texture { get; } = texture;
    public Vector2 UVScale { get; } = uvscale ?? Vector2.One;
    public float UVRotation { get; } = uvrotate;

    public override void Dispose()
    {
        base.Dispose();
        Texture?.Dispose();
        GC.SuppressFinalize(this);
    }
}