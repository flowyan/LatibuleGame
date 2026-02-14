using System.Drawing;
using Latibule.Core;
using Latibule.Core.Physics;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Models;
using Latibule.Core.Rendering.Objects;
using Latibule.Entities;
using Latibule.Gui;
using Latibule.Services;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Latibule.Core.Logger;

namespace Latibule;

public class LatibuleGame : GameWindow
{
    // public static ImGuiRenderer ImGuiRenderer = null!;

    // public static DebugUi DebugUi;
    public static DebugUi3D DebugUi3d;
    public static Player Player { get; private set; }

    // public static FontSystem Fonts { get; private set; }
    public static World GameWorld { get; set; }

    static Shader? shader;
    private PlaneModel? testPlane;

    public LatibuleGame(NativeWindowSettings nativeWindowSettings) : base(GameWindowSettings.Default, nativeWindowSettings)
    {
        LogInfo($"Initializing {Metadata.GAME_NAME} ver{Metadata.GAME_VERSION}");
        GameStateManager.Initialize(this);
        DevConsoleService.Initialize();
        // ImGuiRenderer = new ImGuiRenderer(this);
        // Fonts = new FontSystem();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        UpdateFrequency = GameOptions.TargetFPS;
        VSync = VSyncMode.Off;

        CursorState = CursorState.Grabbed;

        // ImGuiRenderer.RebuildFontAtlas();
        // AssetManager.LoadAssets(Content, GraphicsDevice);
        // AssetManager.PlaySound(SoundAsset.tada, volume: 0.25f, randomPitch: false);

        GLFW.WindowHint(WindowHintInt.DepthBits, 24);

        shader = new Shader(
            "Assets/shader/triangle/shader.vert",
            "Assets/shader/triangle/shader.frag"
        );

        var debugUiShader = new Shader(
            "Assets/shader/debugui/debug_lines.vert",
            "Assets/shader/debugui/debug_lines.frag"
        );
        GL.ClearColor(Color.DimGray);
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Cw);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.ClearDepth(1.0);
        GL.DepthMask(true);

        // TESTING WORLD
        GameWorld = CreateWorld();
        GameWorld.OnLoad();

        // DebugUi = new DebugUi(GraphicsDevice);
        DebugUi3d = new DebugUi3D(debugUiShader);

        Player = new Player(this, new Vector3(0, 0, 0));
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    public static World CreateWorld()
    {
        if (shader == null) throw new Exception("Shader not initialized");
        var world = new World()
        {
            Objects =
            [
                new PlaneModel(shader)
                {
                    Position = new Vector3(0, 0, 0),
                    Scale = new Vector3(10, 0, 10),
                    UVScale = new Vector2(5, 5),
                    Components =
                    [
                    ]
                },
                new CorridorModel(shader) { Position = new Vector3(12, 0, 0) },
                new CorridorModel(shader) { Position = new Vector3(16, 0, 0) }
            ]
        };
        return world;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused)
            return;

        Input.Update(KeyboardState);
        // GameStates.MState = MouseState;

        // Now run game logic that reads input + current physics state
        Player.Update(args);
        GameWorld.OnUpdateFrame(args);

        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GameWorld.OnRenderFrame(args);
        DebugUi3d.OnRenderFrame(Player);

        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GameWorld.Dispose();
        shader?.Dispose();
    }
}