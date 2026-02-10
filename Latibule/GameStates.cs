using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Latibule;

public static class GameStates
{
    // Keyboard and mouse states and mouse handling
    public static KeyboardState PreviousKState { get; set; }
    public static MouseState PreviousMState { get; set; }
    public static bool MouseLocked { get; set; } = true;

    // Game-related properties
    public static GameTime GameTime { get; private set; } = new();

    public static bool ShowHud { get; set; } = true;

    public static void Initialize()
    {
        PreviousKState = new KeyboardState();
        PreviousMState = new MouseState();
    }


}