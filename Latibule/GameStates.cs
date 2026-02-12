using Latibule.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Latibule;

public static class GameStates
{
    // Keyboard and mouse states and mouse handling
    public static KeyboardState KState { get; set; } = new();
    public static MouseState MState { get; set; } = new();
    public static KeyboardState PreviousKState { get; set; }
    public static MouseState PreviousMState { get; set; }
    public static bool MouseLookLocked { get; set; } = false;

    public static bool HasDeveloperKey { get; set; } = false;

    // Game-related properties
    public static Game Game { get; set; } = null!;
    public static GameTime GameTime { get; private set; } = new();
    public static IGuiScreen? CurrentGui { get; set; }

    public static bool ShowHud { get; set; } = true;

    public static void Initialize()
    {
        PreviousKState = new KeyboardState();
        PreviousMState = new MouseState();
    }


}