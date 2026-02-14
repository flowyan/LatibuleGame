using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Latibule.Core.Components;

public class ShaderComponent(Shader shader) : BaseComponent
{
    public Shader Shader { get; } = shader;
    public Texture? Texture { get; init; }
    public Vector2 UVScale { get; init; } = Vector2.One;

    public override void Dispose()
    {
        base.Dispose();
        Shader.Dispose();
        Texture?.Dispose();
        GC.SuppressFinalize(this);
    }
}