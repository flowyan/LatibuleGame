using System.ComponentModel;
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

    private static Shader? shader;

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
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    public static World CreateWorld()
    {
        if (shader == null) throw new Exception("Shader not initialized");
        var world = new World();
        Player = new Player()
        {
            Transform =
            {
                Position = new Vector3(0, 1, 0),
            }
        };
        world.AddObject(Player);
        world.AddObject(new PlaneObject()
        {
            Transform =
            {
                Position = new Vector3(0, 0, 0),
                Scale = new Vector3(10, 0, 10)
            },
        }.WithComponents([
            new ShaderComponent(shader)
            {
                Texture = new Texture($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_TEXTURE_PATH}/material/stone.jpg"),
                UVScale = new Vector2(5, 5)
            },
        ]));
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(12, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(16, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(20, 0, 0) } });

        return world;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;

        Input.Update(KeyboardState);
        // GameStates.MState = MouseState;

        // Now run game logic that reads input + current physics state
        GameWorld.OnUpdateFrame(args);

        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        // CODE HERE //

        GameWorld.OnRenderFrame(args);
        DebugUi3d.OnRenderFrame();

        // --------- //
        DevConsoleService.OnRenderFrame(args, Context);
        SwapBuffers();
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
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        GameWorld.Dispose();
        shader?.Dispose();
    }
}