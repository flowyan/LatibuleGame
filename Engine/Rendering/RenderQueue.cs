using OpenTK.Windowing.Common;

namespace Engine.Rendering;

public static class RenderQueue
{
    public static void OnFrameRender(FrameEventArgs args)
    {
        // World
        LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.World);

        // DebugOutline
        LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.DebugOutline);

        // WorldText
        LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.WorldText);

        // Transparent
        LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.Transparent);

        // Viewmodel
        LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.Viewmodel);

        // UI
        LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.UI);
        // DevConsoleService.OnRenderFrame(args);
    }
}