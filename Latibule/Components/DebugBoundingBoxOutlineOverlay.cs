using System.Drawing;
using Engine;
using Engine.Core;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Rendering;
using Engine.Rendering.Renderer;
using Latibule.Core.Types;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace Latibule.Components;

public class DebugBoundingBoxOutlineOverlay : BaseComponent
{
    private BoundingBoxOutlineRenderer _renderer;

    public DebugBoundingBoxOutlineOverlay()
    {
        RenderLayer = RenderLayer.DebugOutline;
        _renderer = new BoundingBoxOutlineRenderer();
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (!EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes]) return;

        GL.Enable(EnableCap.DepthTest); // occluded by world
        // GL.Disable(EnableCap.DepthTest);  // always visible

        _renderer.Render(LatibuleGame.Player.BoundingBox, Color.White);

        foreach (var boundingBox in LatibuleEngine.Map.GetBoundingBoxes())
            _renderer.Render(boundingBox, Color.Yellow);
    }
}