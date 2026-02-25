using OpenTK.Windowing.Common;

namespace Latibule.Core.Rendering;

public static class RenderQueue
{
    public static void OnFrameRender(FrameEventArgs args)
    {
        // World
        LatibuleGame.GameWorld.OnRenderFrame(args, RenderLayer.World);

        // WorldText
        LatibuleGame.DebugUi3d.Render(); // for text to render correctly with bounding box debug it must be before worldtext
        LatibuleGame.GameWorld.OnRenderFrame(args, RenderLayer.WorldText);

        // Transparent
        LatibuleGame.GameWorld.OnRenderFrame(args, RenderLayer.Transparent);

        // UI
        LatibuleGame.GameWorld.OnRenderFrame(args, RenderLayer.UI);
        LatibuleGame.DebugUi.OnRenderFrame(args);
    }
}