using System.Reflection;
using Latibule.Core;
using Latibule.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Latibule.Services;

public static class GameStateManager
{
    public static void Initialize(Game game)
    {
        GameStates.Initialize();
        GameStates.Game = game;
    }

    public static void Update(Game game, GameTime gameTime)
    {
        GameStates.KState = Keyboard.GetState();
        GameStates.MState = Mouse.GetState();
        var ks = GameStates.KState;

        // if (Input.IsKeyPressedNow(Keys.Escape)) game.Exit();

        if (Input.IsKeyPressedNow(Keys.F1)) GameStates.ShowHud = !GameStates.ShowHud;

        if (Input.IsKeyPressedNow(Keys.F3)) LatibuleGame.DebugUi.ShowDebug = !LatibuleGame.DebugUi.ShowDebug;

        if (Input.AreKeysPressedNow(Keys.F3, Keys.B)) LatibuleGame.DebugUi3d.ShowBoundingBoxes = !LatibuleGame.DebugUi3d.ShowBoundingBoxes;

        if (Input.IsKeyPressedNow(Keys.F11)) LatibuleGame.GDM.ToggleFullScreen();

        if (Input.IsKeyPressedNow(Keys.OemTilde) && GameStates.CurrentGui == null) SetUiOnScreen(new DevConsole());

        GameStates.PreviousMState = GameStates.MState;
        GameStates.PreviousKState = ks;
    }

    public static void SetUiOnScreen(IGuiScreen? gui = null)
    {
        if (gui?.GetType() == GameStates.CurrentGui?.GetType() || gui == null)
        {
            // If the same GUI is requested, toggle it off
            Logger.LogDebug($"Hiding GUI: {GameStates.CurrentGui?.GetType().Name}");
            GameStates.MouseLookLocked = false;
            Mouse.IsRelativeMouseModeEXT = true;
            GameStates.Game.IsMouseVisible = false;
            GameStates.CurrentGui = null;
        }
        else if (GameStates.CurrentGui == null)
        {
            Logger.LogDebug($"Showing GUI: {gui.GetType().Name}");
            GameStates.MouseLookLocked = true;
            Mouse.IsRelativeMouseModeEXT = false;
            GameStates.Game.IsMouseVisible = true;
            GameStates.CurrentGui = gui;
        }
    }
}