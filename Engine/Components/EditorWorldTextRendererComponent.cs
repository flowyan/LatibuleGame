using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Rendering;
using Engine.Rendering.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace Engine.Components;

public class EditorWorldTextRendererComponent : BaseComponent
{
    private WorldTextRenderer? _renderer;
    private readonly TextRendererOptions _options;

    // static text
    public EditorWorldTextRendererComponent(TextRendererOptions options)
    {
        _options = options;
        RenderLayer = RenderLayer.UI;
    }

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);
        _renderer = new WorldTextRenderer(_options);
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        _renderer?.Render(Parent.Transform, _options.text, true);
    }
}