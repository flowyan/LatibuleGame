using Latibule.Core.Types;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Latibule;

public static class GameStates
{
    // Keyboard and mouse states and mouse handling
    public static MouseState MState { get; set; } = null!;
    public static bool MouseLookLocked { get; set; } = false;

    public static bool HasDeveloperKey { get; set; } = false;

    // Game-related properties
    public static GameWindow GameWindow { get; set; } = null!;
    public static IGuiScreen? CurrentGui { get; set; }

    public static bool ShowHud { get; set; } = true;

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