using Assimp;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Renderer;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components;

public class ModelRendererComponent(Scene model) : BaseComponent
{
    private ModelRenderer _renderer = null!;

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);

        var shaderComponent = gameObject.Require<ShaderComponent>();
        var textureComponent = gameObject.Get<TextureComponent>();

        _renderer = new ModelRenderer(
            shaderComponent.Shader,
            model,
            Parent.Transform,
            textureComponent?.Textures,
            textureComponent?.Textures.Length == 1 ? textureComponent.Textures[0] : null
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