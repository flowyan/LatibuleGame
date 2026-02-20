using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Latibule.Core.Components;

public class ShaderComponent(Shader shader) : BaseComponent
{
    public Shader Shader { get; } = shader;

    public override void Dispose()
    {
        base.Dispose();
        Shader.Dispose();
        GC.SuppressFinalize(this);
    }
}