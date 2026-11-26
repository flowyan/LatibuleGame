using Engine.Core.Types;
using Engine.Windowing;

namespace Engine.Core;

public static class EngineStates
{
    public static bool MouseLookLocked { get; set; } = false;
    public static EngineAppWindow GameWindow { get; set; } = null!;

    // Debug related
    public static readonly bool DebugEnv = Environment.GetEnvironmentVariable("debug") == "true";

    public static Dictionary<DebugOverlayType, bool> EnabledDebugOverlays { get; } =
        new(Enum.GetValues<DebugOverlayType>()
            .Select(x => new KeyValuePair<DebugOverlayType, bool>(x, DebugEnv))
        );

    public static void Initialize(EngineAppWindow gameWindow)
    {
    }
}