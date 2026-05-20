using System.ComponentModel;
using System.Drawing;
using Engine.Core;
using Engine.Core.ImGuiNet;
using Engine.Data;
using Engine.Rendering;
using Engine.Rendering.Renderer;
using Engine.Services;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static Engine.Core.Logger;

namespace Engine;

public class EngineWindow : GameWindow
{
    public static readonly bool IS_EDITOR = Environment.GetEnvironmentVariable("MALLET_INSTANCE") is not null;

    private static Asseteer Asseteer { get; } = new(new AsseteerPaths(
        Metadata.ASSETS_ROOT_DIRECTORY,
        Metadata.ASSETS_TEXTURE_PATH,
        Metadata.ASSETS_SOUND_PATH,
        Metadata.ASSETS_SHADER_PATH,
        Metadata.ASSETS_FONT_PATH,
        Metadata.ASSETS_MODEL_PATH
    ));

    public EngineWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
        gameWindowSettings,
        nativeWindowSettings
    )
    {
        DevConsole.Initialize();
        LogInfo($"Initializing {Metadata.ENGINE_NAME} version: {Metadata.ENGINE_VERSION}");
        EngineStateManager.Initialize(this);
        // CenterWindow();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        SetupGLDebug();

        Audio.SteamAudio.PrepareSteamAudio();

        OnSetupImGui();

        Asseteer.LoadAssets();

        LatibuleEngine.Physics.SetupJoltPhysics();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!Asseteer.Loaded) return;
        base.OnUpdateFrame(args);

        EngineStateManager.Update(this);
        EngineStates.MState = MouseState;
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
        OnRenderFrameAfterQueue(args);
        // --------- //
        SwapBuffers();
    }

    protected virtual void OnRenderFrameAfterQueue(FrameEventArgs args)
    {
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        LatibuleEngine.Camera.AspectRatio = (float)e.Width / e.Height;
        LatibuleEngine.Camera.UpdateProjectionMatrix();

        ViewModelRenderer.UpdateProjectionMatrix();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        ImguiImplOpenGL3.Shutdown();
        ImguiImplOpenTK4.Shutdown();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        LatibuleEngine.Map.Dispose();
        Audio.SteamAudio.UnloadSteamAudio();
    }

    private void SetupGLDebug()
    {
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
    }

    protected virtual void OnSetupImGui()
    {
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
    }
}