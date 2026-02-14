using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Windowing.Desktop;

namespace Latibule.Core.Components;

public sealed class ShaderComponent(Shader shader) : BaseComponent()
{
    public Shader Shader { get; } = shader;

    public Texture? Texture { get; set; }

    public override void Dispose()
    {
        Shader.Dispose();
        Texture?.Dispose();
        base.Dispose();
    }
}