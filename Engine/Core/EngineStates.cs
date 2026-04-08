using Engine.Core.Types;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Core;

public static class EngineStates
{
    public static MouseState MState { get; set; } = null!;
    public static bool MouseLookLocked { get; set; } = false;
    public static GameWindow GameWindow { get; set; } = null!;

    // Debug related
    public static readonly bool DebugEnv = Environment.GetEnvironmentVariable("debug") == "true";

    public static Dictionary<DebugOverlayType, bool> EnabledDebugOverlays { get; } =
        new(Enum.GetValues<DebugOverlayType>()
            .Select(x => new KeyValuePair<DebugOverlayType, bool>(x, DebugEnv))
        );

    public static void Initialize(GameWindow gameWindow)
    {
        MState = gameWindow.MouseState;
    }
}