using System.ComponentModel;
using System.Drawing;
using ImGuiNET;
using Latibule.Core;
using Latibule.Core.Data;
using Latibule.Core.ImGuiNet;
using Latibule.Core.Rendering;
using Latibule.Entities;
using Latibule.Services;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static Latibule.Core.Logger;
using Metadata = Latibule.Core.Metadata;

namespace Latibule;

public class LatibuleGame : GameWindow
{
    public static Player Player { get; internal set; }
    public static World GameWorld { get; set; } = new();

    public LatibuleGame(NativeWindowSettings nativeWindowSettings) : base(
        new GameWindowSettings
        {
            UpdateFrequency = GameOptions.TargetFPS,
            Win32SuspendTimerOnDrag = true, // Turning this off gives the player physics a bunch of issues when dragging the window
        },
        nativeWindowSettings
    )
    {
        LogInfo($"Initializing {Metadata.GAME_NAME} version: {Metadata.GAME_VERSION}");
        GameStateManager.Initialize(this);
        DevConsoleService.Initialize();
        CenterWindow();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // call once after context creation
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);

        GL.DebugMessageCallback((source, type, id, severity, length, message, _) =>
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

        // IMGUI
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        ImGui.StyleColorsClassic();

        var style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }

        ImguiImplOpenTK4.Init(this);
        ImguiImplOpenGL3.Init();

        // Load the essential assets
        Asseteer.LoadAssets();
        CursorState = CursorState.Grabbed;
        GameWorld = TestWorld.Create();
        GameWorld.OnLoad();

        // Asseteer.PlaySteamAudioSound(SoundAsset.scarletfire, new Vector3(0, 1, -7.5f), 0.75f);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!Asseteer.Loaded) return;
        base.OnUpdateFrame(args);

        Input.Update(KeyboardState);
        GameStateManager.Update(this);
        GameStates.MState = MouseState;

        GameWorld.OnUpdateFrame(args);

        // Core.SteamAudio.SetListenerPosition(Player.Transform.Position, Player.Camera.Direction, Vector3Direction.Up);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        // CODE HERE //
        RenderQueue.OnFrameRender(args);
        // --------- //
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