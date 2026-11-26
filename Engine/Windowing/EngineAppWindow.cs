// FIXME: This class has a bunch of stuff from OpenTK's NativeWindow class, because I'm used to how it works, but it needs rewriting imo.

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Engine.Core;
using Engine.Core.ImGuiNet;
using Engine.Rendering.Renderer;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SDL3;

namespace Engine.Windowing;

public class EngineAppWindow : SDL.IMainCallbacks<EngineAppWindow>, IDisposable
{
    public static Func<EngineAppWindow>? CreateGame { get; set; }
    public static IntPtr WindowPtr { get; protected set; }
    
    public static ImGuiSDL3 ImGuiSdl3Platform;
    public static ImGuiGLRenderer ImGuiRenderer;

    private int _targetFps = 0;

    /// <summary>Frame rate cap. 0 means uncapped.</summary>
    public int TargetFps
    {
        get => _targetFps;
        set
        {
            _targetFps = Math.Max(value, 0);
            SDL.SetHint(SDL.Hints.MainCallbackRate, _targetFps.ToString());
        }
    }
    
    private CursorState _cursorState = CursorState.Grabbed;

    public CursorState CursorState
    {
        get => _cursorState;
        set
        {
            if (_cursorState == value) return; // the editor sets this every frame
            _cursorState = value;
            if (WindowPtr != IntPtr.Zero) ApplyCursorState();
        }
    }

    private bool _closeRequested;

    private void ApplyCursorState()
    {
        bool grabbed = _cursorState == CursorState.Grabbed;
        SDL.SetWindowMouseGrab(WindowPtr, grabbed);
        SDL.SetWindowRelativeMouseMode(WindowPtr, grabbed);

        if (_cursorState == CursorState.Normal) SDL.ShowCursor();
        else SDL.HideCursor();
    }

    private IntPtr _glContext;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _lastFrameTime;
    private bool _disposed;

    private string _title;

    // Both of these are used to cache the size and location of the window before going into full screen mode.
    // When getting out of full screen mode, the location and size will be set to these value in all states other then minimized.
    private Vector2i _cachedWindowClientSize;
    private Vector2i _cachedWindowLocation;

    /// <summary>
    /// Gets or sets the title of the window.
    /// </summary>/
    public string Title
    {
        get => _title;
        set
        {
            SDL.SetWindowTitle(WindowPtr, value);
            _title = value;
        }
    }

    private WindowState _windowState = WindowState.Normal;

    /// <summary>
    /// Gets or sets the <see cref="WindowState" /> for this window.
    /// </summary>
    public unsafe WindowState WindowState
    {
        get => _windowState;

        set
        {
            // Set the new window state before any potential callback is called,
            // so that the new state is available in for example OnResize.
            // - Noggin_bops 2023-09-25
            WindowState previousWindowState = _windowState;
            _windowState = value;

            if (previousWindowState == WindowState.Fullscreen && value != WindowState.Fullscreen)
            {
                SDL.SetWindowFullscreen(WindowPtr, false);
                _cachedWindowClientSize = ClientSize;
            }

            switch (value)
            {
                case WindowState.Normal:
                    SDL.RestoreWindow(WindowPtr);
                    break;

                case WindowState.Minimized:
                    SDL.MinimizeWindow(WindowPtr);
                    break;

                case WindowState.Maximized:
                    SDL.MaximizeWindow(WindowPtr);
                    break;

                case WindowState.Fullscreen:
                    if (previousWindowState != WindowState.Fullscreen)
                    {
                        _cachedWindowClientSize = ClientSize;
                    }

                    // Monitor* monitor = CurrentMonitor.Handle.ToUnsafePtr<Monitor>();
                    // VideoMode* modePtr = SDL.GetVideoMode(monitor);
                    // SDL.SetWindowMonitor(WindowPtr, monitor, 0, 0, modePtr->Width, modePtr->Height, modePtr->RefreshRate);
                    SDL.SetWindowFullscreen(WindowPtr, true);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets or sets a <see cref="Vector2i" /> structure that contains the external size of this window.
    /// </summary>
    public Vector2i Size
    {
        get
        {
            SDL.GetWindowBordersSize(WindowPtr, out int left, out int top, out int right, out int bottom);
            SDL.GetWindowSize(WindowPtr, out int width, out int height);

            return (width + left + right, height + top + bottom);
        }

        set
        {
            SDL.GetWindowBordersSize(WindowPtr, out int left, out int top, out int right, out int bottom);
            int newWidth = value.X - left - right;
            int newHeight = value.Y - top - bottom;
            // Make sure the values are not negative when 0 size is set.
            newWidth = Math.Max(newWidth, 0);
            newHeight = Math.Max(newHeight, 0);
            SDL.SetWindowSize(WindowPtr, newWidth, newHeight);
        }
    }

    /// <summary>
    /// Gets or sets a <see cref="Vector2i" /> structure that contains the client size of this window.
    /// </summary>
    public Vector2i ClientSize
    {
        get
        {
            SDL.GetWindowSize(WindowPtr, out int width, out int height);
            return (width, height);
        }
        set => SDL.SetWindowSize(WindowPtr, value.X, value.Y);
    }

    private void InitializeNative()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Audio))
            throw new InvalidOperationException($"SDL init failed: {SDL.GetError()}");
        
        TargetFps = _targetFps;

        SetGLAttribute(SDL.GLAttr.ContextMajorVersion, 4);
        SetGLAttribute(SDL.GLAttr.ContextMinorVersion, 6);
        SetGLAttribute(SDL.GLAttr.ContextProfileMask, (int)SDL.GLProfile.Core);
        SetGLAttribute(
            SDL.GLAttr.ContextFlags,
            (int)(SDL.GLContextFlag.Debug | SDL.GLContextFlag.ForwardCompatible));

        SetGLAttribute(SDL.GLAttr.DepthSize, 24);
        SetGLAttribute(SDL.GLAttr.StencilSize, 8);
        SetGLAttribute(SDL.GLAttr.MultisampleBuffers, 1);
        SetGLAttribute(SDL.GLAttr.MultisampleSamples, 4);

        WindowPtr = SDL.CreateWindow(
            Title,
            _cachedWindowClientSize.X,
            _cachedWindowClientSize.Y,
            SDL.WindowFlags.OpenGL |
            SDL.WindowFlags.Resizable |
            SDL.WindowFlags.HighPixelDensity);

        if (WindowPtr == IntPtr.Zero)
            throw new InvalidOperationException($"SDL window creation failed: {SDL.GetError()}");

        _glContext = SDL.GLCreateContext(WindowPtr);

        if (_glContext == IntPtr.Zero)
            throw new InvalidOperationException($"OpenGL context creation failed: {SDL.GetError()}");

        InitializeGlBindings();

        if (!SDL.GLSetSwapInterval(0))
            SDL.LogInfo(SDL.LogCategory.Video, $"Could not disable VSync: {SDL.GetError()}");

        SDL.GetWindowSizeInPixels(WindowPtr, out var width, out var height);
        GL.Viewport(0, 0, width, height);
        
        ApplyCursorState();
    }

    private static void SetGLAttribute(SDL.GLAttr attribute, int value)
    {
        if (!SDL.GLSetAttribute(attribute, value))
            throw new InvalidOperationException(
                $"Could not set OpenGL attribute {attribute}: {SDL.GetError()}");
    }

    private static void InitializeGlBindings()
    {
        // Try to load OpenTK.Graphics assembly.
        Assembly assembly;
        try
        {
            assembly = Assembly.Load("OpenTK.Graphics");
        }
        catch
        {
            // Failed to load graphics, oh well.
            // Up to the user I guess?
            // TODO: Should we expose this load failure to the user better?
            return;
        }

        var provider = new SDLBindingsContext();

        void LoadBindings(string typeNamespace)
        {
            Type type = assembly.GetType($"OpenTK.Graphics.{typeNamespace}.GL");
            if (type == null)
            {
                return;
            }

            MethodInfo load = type.GetMethod("LoadBindings");
            if (load == null)
            {
                throw new MissingMethodException($"OpenTK tried to auto-load the OpenGL bindings. We found the {$"OpenTK.Graphics.{typeNamespace}.GL"} class, but we could not find the 'LoadBindings' method. " +
                                                 $"If you are trying to run a trimmed assembly please add a [DynamicDependency()] attribute to your program, or set NativeWindowSettings.AutoLoadBindings = false and load the OpenGL bindings manually.");
            }

            load?.Invoke(null, new object[] { provider });
        }

        LoadBindings("ES11");
        LoadBindings("ES20");
        LoadBindings("ES30");
        LoadBindings("OpenGL");
        LoadBindings("OpenGL4");
    }

    public EngineAppWindow(SDLNativeWindowSettings settings)
    {
        _title = settings.Title;
        _cachedWindowClientSize = settings.ClientSize;
    }

    public static SDL.AppResult AppInit(
        out EngineAppWindow? appState,
        string[] args)
    {
        appState = null;

        try
        {
            if (CreateGame is null)
                throw new InvalidOperationException("No game factory registered.");

            appState = CreateGame();

            appState.InitializeNative();

            // Context and platform first, so OnLoad can use ImGui
            var context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            ImGuiSdl3Platform = new (WindowPtr, IntPtr.Zero);

            appState.OnLoad(); // add custom ImGui fonts here

            // After OnLoad, because this builds the font atlas
            ImGuiRenderer = new ();

            // Don't count load time in the first frame
            appState._lastFrameTime = appState._clock.Elapsed.TotalSeconds;

            return SDL.AppResult.Continue;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            appState?.Dispose();
            appState = null;

            return SDL.AppResult.Failure;
        }
    }

    public SDL.AppResult AppIterate()
    {
        if (_closeRequested) return SDL.AppResult.Success;

        var now = _clock.Elapsed.TotalSeconds;
        var elapsed = now - _lastFrameTime;
        _lastFrameTime = now;

        var args = new FrameEventArgs(elapsed);

        BeginImGuiFrame(elapsed);

        OnUpdateFrame(args);
        OnRenderFrame(args);

        // ImGui goes on top of the scene, then present
        ImGui.Render();
        ImGuiRenderer.Render(ImGui.GetDrawData());
        SDL.GLSwapWindow(WindowPtr);

        Input.EndFrame();

        return _closeRequested ? SDL.AppResult.Success : SDL.AppResult.Continue;
    }

    private void BeginImGuiFrame(double elapsed)
    {
        var io = ImGui.GetIO();

        // Stop ImGui reacting to the mouse while the game has it grabbed
        if (_cursorState == CursorState.Grabbed) io.ConfigFlags |= ImGuiConfigFlags.NoMouse;
        else io.ConfigFlags &= ~ImGuiConfigFlags.NoMouse;

        if (io.WantTextInput && !SDL.TextInputActive(WindowPtr)) SDL.StartTextInput(WindowPtr);
        else if (!io.WantTextInput && SDL.TextInputActive(WindowPtr)) SDL.StopTextInput(WindowPtr);

        // Otherwise ImGuiSDL3 shows the cursor every frame, overriding CursorState
        if (_cursorState == CursorState.Normal) io.ConfigFlags &= ~ImGuiConfigFlags.NoMouseCursorChange;
        else io.ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange;

        io.DeltaTime = Math.Max((float)elapsed, 0.0001f);
        ImGuiSdl3Platform.NewFrame();
        ImGui.NewFrame();
    }

    public SDL.AppResult AppEvent(ref SDL.Event @event)
    {
        Input.HandleEvent(ref @event);
        ImGuiSdl3Platform.ProcessEvent(@event);
        
        switch ((SDL.EventType)@event.Type)
        {
            case SDL.EventType.WindowPixelSizeChanged:
            {
                SDL.GetWindowSizeInPixels(WindowPtr, out var width, out var height);

                GL.Viewport(0, 0, width, height);
                OnFramebufferResize(new FramebufferResizeEventArgs(width, height));
                return SDL.AppResult.Continue;
            }
            case SDL.EventType.Quit:
                Close();
                return _closeRequested ? SDL.AppResult.Success : SDL.AppResult.Continue;

            default:
                return SDL.AppResult.Continue;
        }
    }
    
    protected virtual void OnLoad()
    {
    }

    protected virtual void OnUpdateFrame(FrameEventArgs args)
    {
        Input.Update();
    }

    protected virtual void OnRenderFrame(FrameEventArgs args)
    {
    }

    protected virtual void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
    }

    protected virtual void OnClosing(CancelEventArgs e)
    {
    }

    /// <summary>
    /// Closes this window.
    /// </summary>
    public virtual void Close()
    {
        var c = new CancelEventArgs();
        OnClosing(c);
        if (!c.Cancel) _closeRequested = true;
    }

    public void AppQuit(SDL.AppResult result)
    {
        Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
        
        // These are null if startup failed part way
        ImGuiRenderer?.Dispose();
        ImGuiSdl3Platform?.Dispose();
        if (ImGui.GetCurrentContext() != IntPtr.Zero) ImGui.DestroyContext();

        if (_glContext != IntPtr.Zero)
        {
            SDL.GLDestroyContext(_glContext);
            _glContext = IntPtr.Zero;
        }

        if (WindowPtr != IntPtr.Zero)
        {
            SDL.DestroyWindow(WindowPtr);
            WindowPtr = IntPtr.Zero;
        }

        // No SDL.Quit() here: the callback runner handles it after AppQuit
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}