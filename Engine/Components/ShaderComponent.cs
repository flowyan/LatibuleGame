using Engine.Core.ECS;
using Engine.Rendering;

namespace Engine.Components;

public class ShaderComponent(Shader shader) : BaseComponent
{
    public Shader Shader { get; set; } = shader;

    public override void Dispose()
    {
        base.Dispose();
        Shader.Dispose();
        GC.SuppressFinalize(this);
    }
}