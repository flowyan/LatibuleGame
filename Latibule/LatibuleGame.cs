using System.ComponentModel;
using System.Drawing;
using ImGuiNET;
using Latibule.Core;
using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.ImGuiNet;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Objects;
using Latibule.Core.Rendering.Shapes;
using Latibule.Entities;
using Latibule.Gui;
using Latibule.Services;
using Latibule.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SteamAudio;
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

    // UI text components that should render on top of everything
    public static List<Action<FrameEventArgs>> UiTextRenderers { get; } = new();

    private static Shader? shader;

    private static IPL.Context iplContext;
    private double _fps;
    private static string _fpstext;

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

        Core.SteamAudio.PrepareSteamAudio();

        UpdateFrequency = GameOptions.TargetFPS;
        VSync = VSyncMode.Off;
        CursorState = CursorState.Grabbed;

        // ImGuiRenderer.RebuildFontAtlas();
        Asseteer.LoadAssets(this);
        // AssetManager.PlaySound(SoundAsset.tada, volume: 0.25f, randomPitch: false);

        shader = new Shader(
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/mesh/shader.vert",
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/mesh/shader.frag"
        );

        var debugUiShader = new Shader(
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/debugui/debug_lines.vert",
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/debugui/debug_lines.frag"
        );

        GL.ClearColor(Color.DimGray);
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Cw);

        // IMGUI
        ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        ImGui.StyleColorsDark();

        ImGuiStylePtr style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }

        ImguiImplOpenTK4.Init(this);
        ImguiImplOpenGL3.Init();

        // TESTING WORLD
        GameWorld = CreateWorld();
        GameWorld.OnLoad();

        // DebugUi = new DebugUi(GraphicsDevice);
        DebugUi3d = new DebugUi3D(debugUiShader);

        Asseteer.PlaySteamAudioSound(SoundAsset.scarletfire, new Vector3(0, 1, -7.5f));
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    public static World CreateWorld()
    {
        if (shader == null) throw new Exception("Shader not initialized");
        var world = new World();
        Player = new Player { Transform = { Position = new Vector3(0, 1, 0) } };
        world.AddObject(Player);

        world.AddObject(new GameObject
        {
            Transform =
            {
                Position = new Vector3(-7.5f, 4, 0),
                Scale = new Vector3(2, 2, 2),
                Rotation = new Vector3(0, 270, 0)
            }
        }.WithComponents(new ShaderComponent(shader), new ShapeRendererComponent(new IsoSphere(8, 16)), new CollisionComponent(), new TextureComponent(Asseteer.GetTexture(TextureAsset.misc_tequila))));

        world.AddObject(new GameObject
        {
            Transform = { Position = new Vector3(0, 1, -7.5f) }
        }.WithComponents(new ShaderComponent(shader), new ShapeRendererComponent(new Cube()), new CollisionComponent(), new TextureComponent(Asseteer.GetTexture(TextureAsset.missing))));


        world.AddObject(new PlaneObject
            {
                Transform = { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 0, 10) }
            }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_tiles), new Vector2(10, 10))));

        // Walls
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, 6), Scale = new Vector3(2, 0, 4), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 4), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, -6), Scale = new Vector3(2, 0, 4), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 4), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(-10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 270) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 10), -90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, 10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(-90, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 10), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, -10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(90, 0, -90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 10), -90f)));

        // Corridor
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(12, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(16, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(20, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(24, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(28, 0, 0) } });

        // Text ui object
        world.AddObject(new GameObject().WithComponents(new TextRendererComponent(() => $"faps: {_fpstext}", 10, 0, 1)));
        world.AddObject(new GameObject().WithComponents(new TextRendererComponent(() => $"player velocity: {Player.Velocity}", 10, 20, 1)));
        world.AddObject(new GameObject().WithComponents(new TextRendererComponent(() => $"player pos: {Player.Transform.Position}", 10, 40, 1)));

        // Lights
        world.AddPointLight(new PointLight() { Position = new Vector3(0, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });

        return world;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;

        Input.Update(KeyboardState);
        GameStates.MState = MouseState;


        // Now run game logic that reads input + current physics state
        GameWorld.OnUpdateFrame(args);

        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        Core.SteamAudio.SetListenerPosition(Player.Transform.Position, Player.Camera.Direction, Vector3Direction.Up);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        // CODE HERE //

        GameWorld.OnRenderFrame(args);
        DebugUi3d.OnRenderFrame();

        // Render UI text on top of everything
        foreach (var renderer in UiTextRenderers)
        {
            renderer(args);
        }

        // --------- //
        DevConsoleService.OnRenderFrame(args, Context);
        SwapBuffers();

        var currentFps = 1.0 / args.Time;
        _fps = _fps * 0.9 + currentFps * 0.1; // smoothing
        _fpstext = $"{(int)_fps}";
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        ImguiImplOpenGL3.Shutdown();
        ImguiImplOpenTK4.Shutdown();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        Player.Camera.AspectRatio = (float)e.Width / e.Height;
        Player.Camera.UpdateProjectionMatrix();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        GameWorld.Dispose();
        shader?.Dispose();
        Core.SteamAudio.UnloadSteamAudio();
    }
}