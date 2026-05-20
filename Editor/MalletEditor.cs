using Editor.Core;
using Editor.Data;
using Editor.Windows;
using Engine;
using Engine.Core;
using Engine.Core.ECS;
using Engine.Core.ImGuiNet;
using Engine.Core.Types;
using Engine.Utilities;
using ImGuiNET;
using JoltPhysicsSharp;
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
    public static IEnumerable<GameObject>? SelectedObjects { get; private set; }

    private readonly EditorWindowSystem _windowSystem = new([
        new HierarchyWindow(),
        new ViewportWindow(),
        new InspectorWindow(),
        new MapSettingsWindow(),
        new BottomTabsWindow()
    ]);

    public MalletEditor(NativeWindowSettings nativeWindowSettings) : base(
        new GameWindowSettings
        {
            UpdateFrequency = EditorOptions.TargetFPS,
            // Win32SuspendTimerOnDrag = true, // Turning this off gives the player physics a bunch of issues when dragging the window
        },
        nativeWindowSettings
    )
    {
        LogInfo($"Initializing {Metadata.EDITOR_NAME} version: {Metadata.EDITOR_VERSION}");
        Environment.SetEnvironmentVariable("MALLET_INSTANCE", "TRUE");
        // GameStateManager.Initialize(this);
        // CenterWindow();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        EditorSceneBootstrap.Initialize(ClientSize.X, ClientSize.Y);
        EditorKeybinds.Initialize();
    }

    protected override void OnSetupImGui()
    {
        // IMGUI
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.Fonts.AddFontFromFileTTF("Assets/font/inter.ttf", 18f);

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

    private static bool UnlockedCamera;

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        var deltaTime = (float)args.Time;

        // EngineOptions-related
        EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes] = EditorOptions.RenderBoundingBoxes;
        if (EditorOptions.UpdateMap) LatibuleEngine.Map.OnUpdateFrame(args);

        switch (EditorConfig.CameraLockMode)
        {
            case EditorConfig.CameraLock.Hold:
                UnlockedCamera = Input.IsMouseDown(MouseButton.Right);
                break;
            case EditorConfig.CameraLock.Toggle:
                if (Input.IsMousePressed(MouseButton.Right)) UnlockedCamera = !UnlockedCamera;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        CursorState = UnlockedCamera ? CursorState.Grabbed : CursorState.Normal;
        if (UnlockedCamera)
        {
            // Allow flying in all directions, ignore gravity and collisions
            Vector3 flyMove = Vector3.Zero;
            var slower = false;
            var forward = Vector3.Normalize(new Vector3(LatibuleEngine.Camera.Direction.X, 0, LatibuleEngine.Camera.Direction.Z));
            var right = Vector3.Normalize(Vector3.Cross(forward, Vector3Direction.Up));
            if (Input.IsKeyDown(Keys.W)) flyMove += forward;
            if (Input.IsKeyDown(Keys.S)) flyMove -= forward;
            if (Input.IsKeyDown(Keys.D)) flyMove += right;
            if (Input.IsKeyDown(Keys.A)) flyMove -= right;
            if (Input.IsKeyDown(Keys.Space)) flyMove += Vector3Direction.Up;
            if (Input.IsKeyDown(Keys.LeftControl)) flyMove += Vector3Direction.Down;
            if (Input.IsKeyDown(Keys.LeftShift)) slower = true;

            var flySpeed = slower ? 6f : 12f; // Flying speed

            if (flyMove != Vector3.Zero) flyMove = Vector3.Normalize(flyMove);
            LatibuleEngine.Camera.Position += flyMove * flySpeed * deltaTime;
            LatibuleEngine.Camera.Update();
        }

        _windowSystem.Update(args);
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

    public static void SelectObject(GameObject? obj)
    {
        if (SelectedObjects != null) // OnUnselect basically
        {
            var defaultColor = new JoltColor(255, 255, 255);
            foreach (var go in SelectedObjects)
            {
                go.DebugColor = defaultColor;
                foreach (var child in go.Children) child.DebugColor = defaultColor;
            }
        }

        if (obj == null)
        {
            SelectedObjects = null;
            return;
        }

        if (obj.Parent != null) obj = obj.Parent;

        obj.DebugColor = new JoltColor(255, 0, 0);

        foreach (var child in obj.Children) child.DebugColor = new JoltColor(0, 0, 255);


        SelectedObjects = null;
        SelectedObjects = [obj];

        if (obj.Children.Any())
        {
            SelectedObjects = obj.Children.Prepend(obj).ToArray();
        }

        Logger.LogDebug($"Selected object(s): {string.Join(", ", SelectedObjects.Select(o => o.ToString()))}");
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // var result = MessageBox.Show(
        //     "Are you sure you want to exit?",
        //     $"Exit {Metadata.EDITOR_NAME}",
        //     MessageBoxButtons.YesNo,
        //     MessageBoxIcon.Warning
        // );
        // if (result == DialogResult.No)
        // {
        //     e.Cancel = true;
        //     return;
        // }


        base.OnClosing(e);

        _windowSystem.Dispose();

        ImguiImplOpenGL3.Shutdown();
        ImguiImplOpenTK4.Shutdown();
    }
}