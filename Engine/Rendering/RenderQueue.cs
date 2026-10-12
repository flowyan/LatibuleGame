using OpenTK.Windowing.Common;

namespace Engine.Rendering;

public static class RenderQueue
{
    public static void OnFrameRender(FrameEventArgs args)
    {
        foreach (var layer in Enum.GetValues<RenderLayer>())
        {
            if (layer == RenderLayer.Skybox) EngineWindow._SkyboxRenderer.Render();
            LatibuleEngine.Map?.OnRenderFrame(args, layer);
            if (layer == RenderLayer.DebugOutline) LatibuleEngine.Physics.OnRenderFrame(args);
        }
        
        // World
        // LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.World);

        // DebugOutline
        // LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.DebugOutline);

        // WorldText
        // LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.WorldText);

        // Transparent
        // LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.Transparent);

        // Viewmodel
        // LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.Viewmodel);

        // UI
        // LatibuleEngine.Map?.OnRenderFrame(args, RenderLayer.UI);
        // DevConsoleService.OnRenderFrame(args);
    }
}