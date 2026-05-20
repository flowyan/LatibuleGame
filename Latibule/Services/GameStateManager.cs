using System.Text;
using Engine.Core;
using Engine.Core.Types;
using Latibule.Commands;
using Latibule.Core;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Latibule.Services;

public static class GameStateManager
{
    public static void Initialize(GameWindow gameWindow)
    {
        // Init binds
        Input.BindComboPressed(
            Keys.Escape,
            gameWindow.Close,
            Keys.LeftShift
        );
        Input.BindKeyPressed(Keys.F1, () => GameStates.ShowHud = !GameStates.ShowHud);
        Input.BindKeyPressed(Keys.F3, () => EngineStates.EnabledDebugOverlays[DebugOverlayType.Info] = !EngineStates.EnabledDebugOverlays[DebugOverlayType.Info]);
        Input.BindComboPressed(
            Keys.B,
            () => EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes] = !EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes],
            Keys.F3
        );
        Input.BindComboPressed(
            Keys.L,
            () => EngineStates.EnabledDebugOverlays[DebugOverlayType.PointLights] = !EngineStates.EnabledDebugOverlays[DebugOverlayType.PointLights],
            Keys.F3
        );
        Input.BindKeyPressed(Keys.F5, () => new ReloadWorld().Execute([]));
        Input.BindKeyPressed(Keys.F11, () => gameWindow.WindowState = gameWindow.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen);

        Input.BindKeyPressed(Keys.GraveAccent, () =>
        {
            if (GameStates.CurrentGui == null) SetUiOnScreen(new DevConsoleWindow(), imgui: true);
        });

        Input.BindKeyPressed(Keys.Escape, () =>
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

    public static void Update(GameWindow gameWindow)
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