using Latibule.Services;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Rendering;

public static class RenderQueue
{
    public static void OnFrameRender(FrameEventArgs args)
    {
        // World
        LatibuleGame.GameWorld?.OnRenderFrame(args, RenderLayer.World);

        // DebugOutline
        LatibuleGame.GameWorld?.OnRenderFrame(args, RenderLayer.DebugOutline);

        // WorldText
        LatibuleGame.GameWorld?.OnRenderFrame(args, RenderLayer.WorldText);

        // Transparent
        LatibuleGame.GameWorld?.OnRenderFrame(args, RenderLayer.Transparent);

        // UI
        LatibuleGame.GameWorld?.OnRenderFrame(args, RenderLayer.UI);
        DevConsoleService.OnRenderFrame(args);
    }
}