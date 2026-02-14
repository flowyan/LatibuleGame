using System.Drawing;
using ImGuiNET;
using Latibule.Core;
using Latibule.Core.Components;
using Latibule.Core.ImGuiNet;
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

        shader = new Shader(
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/triangle/shader.vert",
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/triangle/shader.frag"
        );

        var debugUiShader = new Shader(
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/debugui/debug_lines.vert",
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/debugui/debug_lines.frag"
        );

        GL.ClearColor(Color.DimGray);
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Cw);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.ClearDepth(1.0);
        GL.DepthMask(true);

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
                new PlaneModel()
                {
                    Position = new Vector3(0, 0, 0),
                    Scale = new Vector3(10, 0, 10),
                    UVScale = new Vector2(5, 5),
                    Components =
                    [
                        new ShaderComponent(shader)
                        {
                            Texture = new Texture($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_TEXTURE_PATH}/material/stone.jpg")
                        }
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
        if (!IsFocused) return;

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
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        // CODE HERE //

        GameWorld.OnRenderFrame(args);
        DebugUi3d.OnRenderFrame(Player);

        // --------- //
        DevConsoleService.OnRenderFrame(args, Context);
        SwapBuffers();
    }

    public override void Close()
    {
        ImguiImplOpenGL3.Shutdown();
        ImguiImplOpenTK4.Shutdown();
        base.Close();
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