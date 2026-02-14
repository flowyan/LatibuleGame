using System.Reflection;
using ImGuiNET;
using Latibule.Commands;
using Latibule.Core;
using Latibule.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Latibule.Services;

public static class GameStateManager
{
    private static Action<char>? _textInputHandler;

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

        if (Input.AreKeysPressedNow(Keys.LeftShift, Keys.Escape)) game.Exit();

        if (Input.IsKeyPressedNow(Keys.F1)) GameStates.ShowHud = !GameStates.ShowHud;

        if (Input.IsKeyPressedNow(Keys.F3)) LatibuleGame.DebugUi.ShowDebug = !LatibuleGame.DebugUi.ShowDebug;

        if (Input.AreKeysPressedNow(Keys.F3, Keys.B)) LatibuleGame.DebugUi3d.ShowBoundingBoxes = !LatibuleGame.DebugUi3d.ShowBoundingBoxes;

        if (Input.IsKeyPressedNow(Keys.F5)) new ReloadWorld().Execute([]);

        if (Input.IsKeyPressedNow(Keys.F11)) LatibuleGame.GDM.ToggleFullScreen();

        if (Input.IsKeyPressedNow(Keys.OemTilde) && GameStates.CurrentGui == null) SetUiOnScreen(new DevConsole(), imgui: true);
        if (Input.IsKeyPressedNow(Keys.Escape) && GameStates.CurrentGui != null) SetUiOnScreen();

        GameStates.PreviousMState = GameStates.MState;
        GameStates.PreviousKState = ks;
    }

    public static void SetUiOnScreen(IGuiScreen? gui = null, bool imgui = false)
    {
        if (gui?.GetType() == GameStates.CurrentGui?.GetType() || gui == null)
        {
            // If the same GUI is requested, toggle it off
            Logger.LogDebug($"Hiding GUI: {GameStates.CurrentGui?.GetType().Name}", logToDevConsole: gui is DevConsole);
            GameStates.MouseLookLocked = false;
            Mouse.IsRelativeMouseModeEXT = true;
            GameStates.Game.IsMouseVisible = false;
            GameStates.CurrentGui = null;
            ImGuiStopTextInput();
        }
        else if (GameStates.CurrentGui == null)
        {
            if (imgui) ImGuiStartTextInput();
            gui.Initialize();
            Logger.LogDebug($"Showing GUI: {gui.GetType().Name}", logToDevConsole: gui is not DevConsole);
            GameStates.MouseLookLocked = true;
            Mouse.IsRelativeMouseModeEXT = false;
            GameStates.Game.IsMouseVisible = true;
            GameStates.CurrentGui = gui;
        }
    }

    private static void ImGuiStartTextInput()
    {
        if (_textInputHandler != null) return;

        var io = ImGui.GetIO();
        _textInputHandler = c => LatibuleGame.ImGuiRenderer.OnTextInput(c, io);
        TextInputEXT.TextInput += _textInputHandler;
    }

    private static void ImGuiStopTextInput()
    {
        if (_textInputHandler == null) return;

        TextInputEXT.TextInput -= _textInputHandler;
        _textInputHandler = null;
    }
}