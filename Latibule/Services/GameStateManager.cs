using System.Reflection;
using ImGuiNET;
using Latibule.Commands;
using Latibule.Core;
using Latibule.Core.Types;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Latibule.Services;

public static class GameStateManager
{
    public static void Initialize(GameWindow gameWindow)
    {
        GameStates.Initialize(gameWindow);
        GameStates.GameWindow = gameWindow;
        Input.Initialize(gameWindow.KeyboardState);

        // Init binds
        Input.BindComboPressed(
            Keys.Escape,
            gameWindow.Close,
            Keys.LeftShift
        );
        Input.BindKeyPressed(Keys.F1, () => GameStates.ShowHud = !GameStates.ShowHud);
        Input.BindKeyPressed(Keys.F3, () => GameStates.EnabledDebugOverlays[DebugOverlayType.Info] = !GameStates.EnabledDebugOverlays[DebugOverlayType.Info]);
        Input.BindComboPressed(
            Keys.B,
            () => GameStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes] = !GameStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes],
            Keys.F3
        );
        Input.BindComboPressed(
            Keys.L,
            () => GameStates.EnabledDebugOverlays[DebugOverlayType.PointLights] = !GameStates.EnabledDebugOverlays[DebugOverlayType.PointLights],
            Keys.F3
        );
        Input.BindKeyPressed(Keys.F5, () => new ReloadWorld().Execute([]));
        Input.BindKeyPressed(Keys.F11, () => gameWindow.WindowState = gameWindow.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen);

        Input.BindKeyPressed(Keys.GraveAccent, () =>
        {
            if (GameStates.CurrentGui == null) SetUiOnScreen(new DevConsole(), imgui: true);
        });

        Input.BindKeyPressed(Keys.Escape, () =>
        {
            if (GameStates.CurrentGui != null) SetUiOnScreen();
        });
    }

    public static void Update(GameWindow gameWindow)
    {
        GameStates.GameWindow = gameWindow;
    }

    public static void SetUiOnScreen(IGuiScreen? gui = null, bool imgui = false)
    {
        if (gui?.GetType() == GameStates.CurrentGui?.GetType() || gui == null)
        {
            // If the same GUI is requested, toggle it off
            Logger.LogDebug($"Hiding GUI: {GameStates.CurrentGui?.GetType().Name}", logToDevConsole: gui is DevConsole);
            GameStates.MouseLookLocked = false;
            GameStates.GameWindow.CursorState = CursorState.Grabbed;
            GameStates.CurrentGui = null;
            // ImGuiStopTextInput();
        }
        else if (GameStates.CurrentGui == null)
        {
            gui.Initialize();
            Logger.LogDebug($"Showing GUI: {gui.GetType().Name}", logToDevConsole: gui is not DevConsole);
            GameStates.MouseLookLocked = true;
            GameStates.GameWindow.CursorState = CursorState.Normal;
            GameStates.CurrentGui = gui;
        }
    }
}