using System.Text;
using Engine.Core;
using Engine.Core.Types;
using Engine.Windowing;
using Latibule.Commands;
using Latibule.Core;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SDL3;

namespace Latibule.Services;

public static class GameStateManager
{
    public static void Initialize(EngineAppWindow gameWindow)
    {
        // Init binds
        Input.BindComboPressed(
            SDL.Scancode.Escape,
            gameWindow.Close,
            SDL.Scancode.LShift
        );
        Input.BindKeyPressed(SDL.Scancode.F1, () => GameStates.ShowHud = !GameStates.ShowHud);
        Input.BindKeyPressed(SDL.Scancode.F3, () => EngineStates.EnabledDebugOverlays[DebugOverlayType.Info] = !EngineStates.EnabledDebugOverlays[DebugOverlayType.Info]);
        Input.BindComboPressed(
            SDL.Scancode.B,
            () => EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes] = !EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes],
            SDL.Scancode.F3
        );
        Input.BindComboPressed(
            SDL.Scancode.L,
            () => EngineStates.EnabledDebugOverlays[DebugOverlayType.PointLights] = !EngineStates.EnabledDebugOverlays[DebugOverlayType.PointLights],
            SDL.Scancode.F3
        );
        Input.BindKeyPressed(SDL.Scancode.F5, () => new ReloadWorld().Execute([]));
        Input.BindKeyPressed(SDL.Scancode.F11, () => gameWindow.WindowState = gameWindow.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen);

        Input.BindKeyPressed(SDL.Scancode.Grave, () =>
        {
            if (GameStates.CurrentGui == null) SetUiOnScreen(new DevConsoleWindow(), imgui: true);
        });

        Input.BindKeyPressed(SDL.Scancode.Escape, () =>
        {
            if (GameStates.CurrentGui != null) SetUiOnScreen();
        });

        // Check for a developer key
        var keyBase64 = Convert.FromBase64String("aWFtdGhlb25ld2hva25vY2tz");
        var key = Encoding.UTF8.GetString(keyBase64);
        if (File.Exists("key.txt")) GameStates.HasDeveloperKey = File.ReadAllText("key.txt") == key;
        if (Environment.GetEnvironmentVariable("DEV_KEY") != null) GameStates.HasDeveloperKey = Environment.GetEnvironmentVariable("DEV_KEY") == key;
        if (GameStates.HasDeveloperKey) Logger.LogInfo("Developer key found. Developer tools granted.");
    }

    public static void Update(EngineAppWindow engineAppWindow)
    {
        DevConsole.IsOpen = GameStates.CurrentGui is DevConsoleWindow;
    }

    public static void SetUiOnScreen(IGuiScreen? gui = null, bool imgui = false)
    {
        if (gui?.GetType() == GameStates.CurrentGui?.GetType() || gui == null)
        {
            // If the same GUI is requested, toggle it off
            Logger.LogDebug($"Hiding GUI: {GameStates.CurrentGui?.GetType().Name}", logToDevConsole: gui is DevConsoleWindow);
            EngineStates.MouseLookLocked = false;
            EngineStates.GameWindow.CursorState = CursorState.Grabbed;
            GameStates.CurrentGui = null;
        }
        else if (GameStates.CurrentGui == null)
        {
            gui.Initialize();
            Logger.LogDebug($"Showing GUI: {gui.GetType().Name}", logToDevConsole: gui is not DevConsoleWindow);
            EngineStates.MouseLookLocked = true;
            EngineStates.GameWindow.CursorState = CursorState.Normal;
            GameStates.CurrentGui = gui;
        }
    }
}