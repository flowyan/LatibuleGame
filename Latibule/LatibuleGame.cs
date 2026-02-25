using System.ComponentModel;
using System.Drawing;
using ImGuiNET;
using Latibule.Core;
using Latibule.Core.Data;
using Latibule.Core.ImGuiNet;
using Latibule.Core.Rendering;
using Latibule.Entities;
using Latibule.Gui;
using Latibule.Services;
using Latibule.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static Latibule.Core.Logger;
using Metadata = Latibule.Core.Metadata;

namespace Latibule;

public class LatibuleGame : GameWindow
{
    public static DebugUi DebugUi;
    public static DebugUi3D DebugUi3d;
    public static Player Player { get; internal set; }
    public static World GameWorld { get; set; }

    public LatibuleGame(NativeWindowSettings nativeWindowSettings) : base(GameWindowSettings.Default, nativeWindowSettings)
    {
        LogInfo($"Initializing {Metadata.GAME_NAME} version: {Metadata.GAME_VERSION}");
        GameStateManager.Initialize(this);
        DevConsoleService.Initialize();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // call once after context creation
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);

        GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
        {
            var msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length);
            LogWarning($"[GL DEBUG] {severity} {type} {source} (id={id}): {msg}");
        }, IntPtr.Zero);

        // optional: don't spam notifications
        GL.DebugMessageControl(DebugSourceControl.DontCare,
            DebugTypeControl.DontCare,
            DebugSeverityControl.DebugSeverityNotification,
            0, Array.Empty<int>(), false);

        Core.SteamAudio.PrepareSteamAudio();

        UpdateFrequency = GameOptions.TargetFPS;
        VSync = VSyncMode.Off;
        CursorState = CursorState.Grabbed;

        Asseteer.LoadAssets(this);
        Asseteer.PlaySound(SoundAsset.tada, volume: 0.25f, randomPitch: false);

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
        GameWorld = TestWorld.Create();
        GameWorld.OnLoad();

        DebugUi = new DebugUi();
        DebugUi3d = new DebugUi3D();

        Asseteer.PlaySteamAudioSound(SoundAsset.scarletfire, new Vector3(0, 1, -7.5f), 0.75f);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;

        Input.Update(KeyboardState);
        GameStateManager.Update(this);
        GameStates.MState = MouseState;

        // Now run game logic that reads input + current physics state
        GameWorld.OnUpdateFrame(args);

        Core.SteamAudio.SetListenerPosition(Player.Transform.Position, Player.Camera.Direction, Vector3Direction.Up);

        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        // CODE HERE //
        RenderQueue.OnFrameRender(args);
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
        Player.Camera.AspectRatio = (float)e.Width / e.Height;
        Player.Camera.UpdateProjectionMatrix();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        GameWorld.Dispose();
        Core.SteamAudio.UnloadSteamAudio();
    }
}