using Engine.Core;
using OpenTK.Windowing.Desktop;

namespace Engine.Services;

public static class EngineStateManager
{
    public static void Initialize(GameWindow gameWindow)
    {
        EngineStates.Initialize(gameWindow);
        Input.Initialize(gameWindow.KeyboardState, gameWindow.MouseState);
        EngineStates.GameWindow =  gameWindow;
    }

    public static void Update(GameWindow gameWindow)
    {
        EngineStates.GameWindow = gameWindow;
        Input.Update(gameWindow.KeyboardState, gameWindow.MouseState);
    }
}