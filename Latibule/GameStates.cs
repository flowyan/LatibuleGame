using Engine.Core.Types;

namespace Latibule;

public static class GameStates
{
    public static bool HasDeveloperKey { get; set; } = false;

    // Game-related properties
    public static IGuiScreen? CurrentGui { get; set; }

    public static bool ShowHud { get; set; } = true;
}