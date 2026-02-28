using System.Drawing;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Renderer;
using Latibule.Core.Types;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components.Dev;

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
        if (!GameStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes]) return;

        GL.Enable(EnableCap.DepthTest); // occluded by world
        // GL.Disable(EnableCap.DepthTest);  // always visible

        _renderer.Render(LatibuleGame.Player.BoundingBox, Color.White);

        foreach (var boundingBox in LatibuleGame.GameWorld.GetBoundingBoxes())
            _renderer.Render(boundingBox, Color.Yellow);
    }
}