using FontStashSharp;
using Latibule.Core;
using Latibule.Core.Data;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Objects;
using Latibule.Entities;
using Latibule.Gui;
using Latibule.Models;
using Latibule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Latibule.Core.Logger;
using Plane = Latibule.Core.Rendering.Objects.Plane;

namespace Latibule;

public class LatibuleGame : Game
{
    public static GraphicsDeviceManager GDM = null!;
    public static ImGuiRenderer ImGuiRenderer = null!;

    public static DebugUi DebugUi;
    public static DebugUi3D DebugUi3d;
    public static Player Player { get; private set; }
    public static FontSystem Fonts { get; private set; }

    public static World GameWorld { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        using var g = new LatibuleGame();
        g.Run();
    }

    private LatibuleGame()
    {
        GDM = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            IsFullScreen = GameOptions.FullScreen,
            SynchronizeWithVerticalRetrace = false,
            PreferMultiSampling = true,
        };

        Content.RootDirectory = Metadata.ASSETS_ROOT_DIRECTORY;

        Window.AllowUserResizing = true;
        IsMouseVisible = false;
        Mouse.IsRelativeMouseModeEXT = true;

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / GameOptions.TargetFPS);
    }

    protected override void Initialize()
    {
        LogInfo($"Initializing {Metadata.GAME_NAME} ver{Metadata.GAME_VERSION}");
        GameStateManager.Initialize(this);
        DevConsoleService.Initialize();
        ImGuiRenderer = new ImGuiRenderer(this);
        Fonts = new FontSystem();

        DebugUi = new DebugUi(GraphicsDevice);
        DebugUi3d = new DebugUi3D(GraphicsDevice);
        // TESTING WORLD
        GameWorld = new World()
        {
            Objects =
            [
                new Plane(this) { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 0, 10), UVScale = new Vector2(5, 5) },
                new Corridor(this) { Position = new Vector3(0, 2, 0) }
            ]
        };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        ImGuiRenderer.RebuildFontAtlas();
        AssetManager.LoadAssets(Content, GraphicsDevice);
        AssetManager.PlaySound(SoundAsset.tada, volume: 0.25f, randomPitch: false);

        base.LoadContent();
    }

    protected override void UnloadContent()
    {
        // Clean up after yourself!
        AssetManager.UnloadAssets();

        base.UnloadContent();
    }

    protected override void BeginRun()
    {
        Player = new Player(GraphicsDevice, new Vector3(0, 0, 0));
        GameWorld.Initialize();

        base.BeginRun();
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            // unlock the mouse if the game is not active
            if (GameStates.MouseLookLocked)
            {
                GameStates.MouseLookLocked = false;
                IsMouseVisible = true;
            }

            return;
        }

        if (GameStates.CurrentGui is DevConsole) return;

        Player.Update(gameTime);
        GameWorld.Update(gameTime);
        GameStateManager.Update(this, gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render stuff in here. Do NOT run game logic in here!
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GameWorld.Draw(gameTime);

        DebugUi.Draw(gameTime);
        DebugUi3d.Draw(Player);

        GameStates.CurrentGui?.Draw(gameTime);
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        GameWorld.Dispose();
        base.Dispose(disposing);
    }
}