using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Windowing.Desktop;

namespace Latibule.Core.Components;

public sealed class ShaderComponent(GameWindow gameWindow, string vertPath, string fragPath) : BaseComponent(gameWindow)
{
    public Shader Shader { get; } = new(vertPath, fragPath);

    public override void Dispose()
    {
        Shader.Dispose();
        base.Dispose();
    }
}