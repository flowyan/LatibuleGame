using FontStashSharp;
using Latibule.Core;
using Latibule.Core.Data;
using Latibule.Core.Rendering;
using Latibule.Entities;
using Latibule.Gui;
using Latibule.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Latibule.Core.Logger;
using Plane = Latibule.Core.Rendering.Objects.Plane;

namespace Latibule;

public class LatibuleGame : Game
{
    public static ImGuiRenderer ImGuiRenderer = null!;


    public static DebugUi DebugUi;
    public static Player Player { get; private set; }
    public static FontSystem Fonts { get; private set; }

    public static World GameWorld { get; private set; }

    private Point _screenCenter;

    [STAThread]
    public static void Main(string[] args)
    {
        using var g = new LatibuleGame();
        g.Run();
    }

    private LatibuleGame()
    {
        new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = true
        };

        Content.RootDirectory = Metadata.ASSETS_ROOT_DIRECTORY;
    }

    protected override void Initialize()
    {
        LogInfo($"Initializing {Metadata.GAME_NAME} ver{Metadata.GAME_VERSION}");
        ImGuiRenderer = new ImGuiRenderer(this);
        Fonts = new FontSystem();

        // Store the center of the screen for mouse handling
        _screenCenter = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

        // Set the window name
        // Window.Title = GetWindowTitle();
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (sender, args) => { _screenCenter = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2); };

        GameStates.Initialize();

        DebugUi = new DebugUi(GraphicsDevice);
        // TESTING WORLD
        GameWorld = new World()
        {
            Objects =
            [
                new Plane(this) { Position = new Vector3(0, 0, 0), Scale =  new Vector3(10, 1, 10), UVScale = new Vector2(5, 5)},
                new Plane(this) { Position = new Vector3(20, 0, 10) },
                new Plane(this) { Position = new Vector3(10, -5, 5) }
            ]
        };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        Console.WriteLine($"Loading {Metadata.GAME_NAME} v{Metadata.GAME_VERSION}");
        ImGuiRenderer.RebuildFontAtlas();
        AssetManager.LoadAssets(Content, GraphicsDevice);
        AssetManager.PlaySound("tada", volume: 0.25f, randomPitch: false);


        base.LoadContent();
    }

    protected override void UnloadContent()
    {
        // Clean up after yourself!
        base.UnloadContent();
    }

    protected override void BeginRun()
    {
        Player = new Player(GraphicsDevice, new Vector3(0, 0, 0));
        GameWorld.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
        GameWorld.Update(gameTime);

        var ks = Keyboard.GetState();
        if (ks.IsKeyDown(Keys.Escape) && GameStates.PreviousKState.IsKeyUp(Keys.Escape)) Exit();

        if (ks.IsKeyDown(Keys.F3) && !GameStates.PreviousKState.IsKeyDown(Keys.F3) && !ks.IsKeyDown(Keys.B))
            DebugUi.ShowDebug = !DebugUi.ShowDebug;

        if (GameStates.MouseLocked)
        {
            Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
            // Update the player's previous mouse state to match the centered position
            GameStates.PreviousMState = new MouseState(
                _screenCenter.X,
                _screenCenter.Y,
                0,
                GameStates.PreviousMState.LeftButton,
                GameStates.PreviousMState.MiddleButton,
                GameStates.PreviousMState.RightButton,
                GameStates.PreviousMState.XButton1,
                GameStates.PreviousMState.XButton2
            );
        }


        GameStates.PreviousKState = ks;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render stuff in here. Do NOT run game logic in here!
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GameWorld.Draw(gameTime);

        DebugUi.Draw(gameTime);
        base.Draw(gameTime);
    }
}