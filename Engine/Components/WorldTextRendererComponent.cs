using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Rendering;
using Engine.Rendering.Renderer;
using OpenTK.Windowing.Common;

namespace Engine.Components;

public class WorldTextRendererComponent : BaseComponent
{
    private WorldTextRenderer? _renderer;
    private readonly TextRendererOptions _options;

    // static text
    public WorldTextRendererComponent(TextRendererOptions options)
    {
        _options = options;
        RenderLayer = RenderLayer.WorldText;
    }

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);
        _renderer = new WorldTextRenderer(_options);
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        _renderer?.Render(Parent.Transform, _options.text);
    }
}