using Editor.Core;
using Editor.Windows;
using Engine;
using Engine.Core;
using Engine.Core.ImGuiNet;
using Engine.Utilities;
using ImGuiNET;
using Latibule.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Engine.Core.Logger;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using Metadata = Editor.Core.Metadata;

namespace Editor;

public class MalletEditor : EngineWindow
{
    private readonly EditorWindowSystem _windowSystem = new([
        new HierarchyWindow(),
        new ViewportWindow(),
        new InspectorWindow(),
        new ConsoleWindow(),
    ]);

    public MalletEditor(NativeWindowSettings nativeWindowSettings) : base(
        new GameWindowSettings
        {
            UpdateFrequency = EditorOptions.TargetFPS,
            Win32SuspendTimerOnDrag = true, // Turning this off gives the player physics a bunch of issues when dragging the window
        },
        nativeWindowSettings
    )
    {
        LogInfo($"Initializing {Metadata.EDITOR_NAME} version: {Metadata.EDITOR_VERSION}");
        Environment.SetEnvironmentVariable("MALLET_INSTANCE", "TRUE");
        // GameStateManager.Initialize(this);
        CenterWindow();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        EditorSceneBootstrap.Initialize(ClientSize.X, ClientSize.Y);
    }

    protected override void OnSetupImGui()
    {
        // IMGUI
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.Fonts.AddFontFromFileTTF(@"C:\Windows\Fonts\segoeui.ttf", 18f);

        ImGui.StyleColorsDark();

        var style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }

        ImguiImplOpenTK4.Init(this);
        ImguiImplOpenGL3.Init();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        var deltaTime = (float)args.Time;

        var UnlockedCamera = MouseState.IsButtonDown(MouseButton.Right);
        CursorState = UnlockedCamera ? CursorState.Grabbed : CursorState.Normal;

        if (UnlockedCamera)
        {
            // Allow flying in all directions, ignore gravity and collisions
            Vector3 flyMove = Vector3.Zero;
            var flySpeed = 12f; // Flying speed
            var forward = Vector3.Normalize(new Vector3(LatibuleEngine.Camera.Direction.X, 0, LatibuleEngine.Camera.Direction.Z));
            var right = Vector3.Normalize(Vector3.Cross(forward, Vector3Direction.Up));
            if (Input.IsKeyDown(Keys.W)) flyMove += forward;
            if (Input.IsKeyDown(Keys.S)) flyMove -= forward;
            if (Input.IsKeyDown(Keys.D)) flyMove += right;
            if (Input.IsKeyDown(Keys.A)) flyMove -= right;
            if (Input.IsKeyDown(Keys.Space)) flyMove += Vector3Direction.Up;
            if (Input.IsKeyDown(Keys.LeftShift)) flyMove += Vector3Direction.Down;
            if (flyMove != Vector3.Zero) flyMove = Vector3.Normalize(flyMove);
            LatibuleEngine.Camera.Position += flyMove * flySpeed * deltaTime;
            LatibuleEngine.Camera.Update();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        GL.ClearColor(0.05f, 0.05f, 0.07f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        ImguiImplOpenGL3.NewFrame();
        ImguiImplOpenTK4.NewFrame();
        ImGui.NewFrame();

        _windowSystem.Render(e);

        ImGui.Render();
        ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
        SwapBuffers();
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to exit?",
            $"Exit {Metadata.EDITOR_NAME}",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        if (result == DialogResult.No)
        {
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);

        _windowSystem.Dispose();

        ImguiImplOpenGL3.Shutdown();
        ImguiImplOpenTK4.Shutdown();
    }
}