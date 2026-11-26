using Engine.Core;
using Engine.Windowing;

namespace Engine.Services;

public static class EngineStateManager
{
    public static void Initialize(EngineAppWindow gameWindow)
    {
        EngineStates.Initialize(gameWindow);
        EngineStates.GameWindow =  gameWindow;
    }

    public static void Update(EngineAppWindow gameWindow)
    {
        EngineStates.GameWindow = gameWindow;
    }
}