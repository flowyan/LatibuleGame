using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Helpers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components;

public class ShapeRendererComponent(Shape shape) : BaseComponent
{
    private ShapeRenderer _renderer = null!;

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);

        var shaderComponent = gameObject.Require<ShaderComponent>();
        var textureComponent = gameObject.Get<TextureComponent>();

        _renderer = new ShapeRenderer(
            shaderComponent.Shader,
            shape,
            Parent.Transform,
            textureComponent?.Textures[0],
            textureComponent?.UVScale ?? Vector2.One,
            textureComponent?.UVRotation ?? 0f
        );
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        _renderer.Render();
    }

    public override void Dispose()
    {
        base.Dispose();
        _renderer.Dispose();
    }
}