using System.Runtime.InteropServices;
using ImGuiNET;
using SDL3;

namespace Engine.Core.ImGuiNet;

/// <summary>
/// Implementation of SDL3 platform backend for ImGui.
/// https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_sdl3.h
/// </summary>
public class ImGuiSDL3 : IDisposable
{
    public readonly nint Window;
    public readonly nint Renderer;
    public readonly uint WindowId;
    uint _mouseWindowId;
    int _mousePendingLeaveFrame;
    nint[] _mouseCursors = new nint[(int)ImGuiMouseCursor.COUNT];
    nint _mouseLastCursor = -1;
    int _mouseButtonsDown = 0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SetClipboardFn(nint context, nint text);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint GetClipboardFn(nint context);

    // Stored in fields so the GC doesn't collect them while ImGui holds the pointers
    private readonly SetClipboardFn _setClipboard;
    private readonly GetClipboardFn _getClipboard;
    private nint _clipboardText;

    public ImGuiSDL3(nint window, nint renderer)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
        Window = window;
        WindowId = SDL.GetWindowID(Window);
        Renderer = renderer;

        ImGuiPlatformIOPtr platformIo = ImGui.GetPlatformIO();
        _setClipboard = (_, text) => SDL.SetClipboardText(Marshal.PtrToStringUTF8(text) ?? "");
        _getClipboard = _ =>
        {
            // ImGui reads this after we return, so keep it alive until the next call
            Marshal.FreeCoTaskMem(_clipboardText);
            _clipboardText = Marshal.StringToCoTaskMemUTF8(SDL.GetClipboardText());
            return _clipboardText;
        };
        platformIo.Platform_SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_setClipboard);
        platformIo.Platform_GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_getClipboard);

        _mouseCursors[(int)ImGuiMouseCursor.Arrow] = SDL.CreateSystemCursor(SDL.SystemCursor.Default);
        _mouseCursors[(int)ImGuiMouseCursor.TextInput] = SDL.CreateSystemCursor(SDL.SystemCursor.Text);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeAll] = SDL.CreateSystemCursor(SDL.SystemCursor.Move);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeNS] = SDL.CreateSystemCursor(SDL.SystemCursor.NSResize);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeEW] = SDL.CreateSystemCursor(SDL.SystemCursor.EWResize);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeNESW] = SDL.CreateSystemCursor(SDL.SystemCursor.NESWResize);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeNWSE] = SDL.CreateSystemCursor(SDL.SystemCursor.NWSEResize);
        _mouseCursors[(int)ImGuiMouseCursor.Hand] = SDL.CreateSystemCursor(SDL.SystemCursor.Pointer);
        _mouseCursors[(int)ImGuiMouseCursor.NotAllowed] = SDL.CreateSystemCursor(SDL.SystemCursor.NotAllowed);

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        SetupPlatformHandles(viewport, window);
    }

    public void Dispose()
    {
        for (int i = 0; i < _mouseCursors.Length; i++)
            SDL.DestroyCursor(_mouseCursors[i]);

        ImGuiPlatformIOPtr platformIo = ImGui.GetPlatformIO();
        platformIo.Platform_SetClipboardTextFn = 0;
        platformIo.Platform_GetClipboardTextFn = 0;

        Marshal.FreeCoTaskMem(_clipboardText);
        _clipboardText = 0;
    }

    public void NewFrame()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        int w, h;
        int displayW, displayH;

        SDL.GetWindowSize(Window, out w, out h);
        if (SDL.GetWindowFlags(Window).HasFlag(SDL.WindowFlags.Minimized))
        {
            w = h = 0;
        }

        SDL.GetWindowSizeInPixels(Window, out displayW, out displayH);
        io.DisplaySize = new System.Numerics.Vector2((float)w, (float)h);

        if (w > 0 && h > 0)
            io.DisplayFramebufferScale = new System.Numerics.Vector2((float)displayW / w, (float)displayH / h);

        if (_mousePendingLeaveFrame > 0 && _mousePendingLeaveFrame >= ImGui.GetFrameCount())
        {
            _mouseWindowId = 0;
            _mousePendingLeaveFrame = 0;
            io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
        }

        UpdateMouseData();
        UpdateMouseCursor();
    }

    public bool ProcessEvent(SDL.Event e)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        switch ((SDL.EventType)e.Type)
        {
            case SDL.EventType.MouseMotion:
                if (e.Motion.WindowID != WindowId)
                    return false;

                io.AddMouseSourceEvent(e.Motion.Which == SDL.TouchMouseID ? ImGuiMouseSource.TouchScreen : ImGuiMouseSource.Mouse);
                io.AddMousePosEvent(e.Motion.X, e.Motion.Y);
                return true;
            case SDL.EventType.MouseWheel:
                if (e.Wheel.WindowID != WindowId)
                    return false;

                float wheelX = -e.Wheel.X;
                float wheelY = e.Wheel.Y;

                io.AddMouseSourceEvent(e.Wheel.Which == SDL.TouchMouseID ? ImGuiMouseSource.TouchScreen : ImGuiMouseSource.Mouse);
                io.AddMouseWheelEvent(wheelX, wheelY);
                return true;
            case SDL.EventType.MouseButtonDown:
            case SDL.EventType.MouseButtonUp:
                if (e.Button.WindowID != WindowId)
                    return false;

                int mouseButton = -1;
                if (e.Button.Button == SDL.ButtonLeft) mouseButton = 0;
                if (e.Button.Button == SDL.ButtonRight) mouseButton = 1;
                if (e.Button.Button == SDL.ButtonMiddle) mouseButton = 2;
                if (e.Button.Button == SDL.ButtonX1) mouseButton = 3;
                if (e.Button.Button == SDL.ButtonX2) mouseButton = 4;
                if (mouseButton == -1) break;

                io.AddMouseSourceEvent(e.Button.Which == SDL.TouchMouseID ? ImGuiMouseSource.TouchScreen : ImGuiMouseSource.Mouse);
                io.AddMouseButtonEvent(mouseButton, (SDL.EventType)e.Type == SDL.EventType.MouseButtonDown);
                _mouseButtonsDown = ((SDL.EventType)e.Type == SDL.EventType.MouseButtonDown) ? _mouseButtonsDown | (1 << mouseButton) : _mouseButtonsDown & ~(1 << mouseButton);
                return true;
            case SDL.EventType.TextInput:
                if (e.Text.WindowID != WindowId)
                    return false;

                unsafe
                {
                    ImGuiNative.ImGuiIO_AddInputCharactersUTF8(io, (byte*)e.Text.Text);
                }

                return true;
            case SDL.EventType.KeyDown:
            case SDL.EventType.KeyUp:
                if (e.Key.WindowID != WindowId)
                    return false;

                UpdateKeyModifiers(e.Key.Mod);
                ImGuiKey key = KeyEventToImGui(e.Key.Key, e.Key.Scancode);
                io.AddKeyEvent(key, (SDL.EventType)e.Type == SDL.EventType.KeyDown);
                io.SetKeyEventNativeData(key, (int)e.Key.Key, (int)e.Key.Scancode, (int)e.Key.Scancode);
                return true;
            case SDL.EventType.WindowMouseEnter:
                if (e.Window.WindowID != WindowId)
                    return false;

                _mouseWindowId = e.Window.WindowID;
                _mousePendingLeaveFrame = 0;
                return true;
            case SDL.EventType.WindowMouseLeave:
                if (e.Window.WindowID != WindowId)
                    return false;

                _mousePendingLeaveFrame = ImGui.GetFrameCount() + 1;
                return true;
            case SDL.EventType.WindowFocusGained:
            case SDL.EventType.WindowFocusLost:
                if (e.Window.WindowID != WindowId)
                    return false;

                io.AddFocusEvent((SDL.EventType)e.Type == SDL.EventType.WindowFocusGained);
                return true;
            default:
                break;
        }

        return false;
    }

    private void UpdateMouseData()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        var focusedWindow = SDL.GetKeyboardFocus();
        bool isAppFocused = focusedWindow == Window;
        if (isAppFocused)
        {
            if (io.WantSetMousePos)
            {
                SDL.WarpMouseInWindow(Window, (int)io.MousePos.X, (int)io.MousePos.Y);
            }
        }
    }

    private void UpdateMouseCursor()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
            return;

        ImGuiMouseCursor imguiCursor = ImGui.GetMouseCursor();

        if (io.MouseDrawCursor || imguiCursor == ImGuiMouseCursor.None)
        {
            SDL.HideCursor();
        }
        else
        {
            nint expectedCursor = _mouseCursors[(int)imguiCursor];
            if (_mouseLastCursor != expectedCursor)
            {
                SDL.SetCursor(expectedCursor);
                _mouseLastCursor = expectedCursor;
            }

            SDL.ShowCursor();
        }
    }

    private ImGuiKey KeyEventToImGui(SDL.Keycode keycoade, SDL.Scancode scancode)
    {
        switch (scancode)
        {
            case SDL.Scancode.Kp0: return ImGuiKey.Keypad0;
            case SDL.Scancode.Kp1: return ImGuiKey.Keypad1;
            case SDL.Scancode.Kp2: return ImGuiKey.Keypad2;
            case SDL.Scancode.Kp3: return ImGuiKey.Keypad3;
            case SDL.Scancode.Kp4: return ImGuiKey.Keypad4;
            case SDL.Scancode.Kp5: return ImGuiKey.Keypad5;
            case SDL.Scancode.Kp6: return ImGuiKey.Keypad6;
            case SDL.Scancode.Kp7: return ImGuiKey.Keypad7;
            case SDL.Scancode.Kp8: return ImGuiKey.Keypad8;
            case SDL.Scancode.Kp9: return ImGuiKey.Keypad9;
            case SDL.Scancode.KpPeriod: return ImGuiKey.KeypadDecimal;
            case SDL.Scancode.KpDivide: return ImGuiKey.KeypadDivide;
            case SDL.Scancode.KpMultiply: return ImGuiKey.KeypadMultiply;
            case SDL.Scancode.KpMinus: return ImGuiKey.KeypadSubtract;
            case SDL.Scancode.KpPlus: return ImGuiKey.KeypadAdd;
            case SDL.Scancode.KpEnter: return ImGuiKey.KeypadEnter;
            case SDL.Scancode.KpEquals: return ImGuiKey.KeypadEqual;
            default: break;
        }

        switch (keycoade)
        {
            case SDL.Keycode.Tab: return ImGuiKey.Tab;
            case SDL.Keycode.Left: return ImGuiKey.LeftArrow;
            case SDL.Keycode.Right: return ImGuiKey.RightArrow;
            case SDL.Keycode.Up: return ImGuiKey.UpArrow;
            case SDL.Keycode.Down: return ImGuiKey.DownArrow;
            case SDL.Keycode.Pageup: return ImGuiKey.PageUp;
            case SDL.Keycode.Pagedown: return ImGuiKey.PageDown;
            case SDL.Keycode.Home: return ImGuiKey.Home;
            case SDL.Keycode.End: return ImGuiKey.End;
            case SDL.Keycode.Insert: return ImGuiKey.Insert;
            case SDL.Keycode.Delete: return ImGuiKey.Delete;
            case SDL.Keycode.Backspace: return ImGuiKey.Backspace;
            case SDL.Keycode.Space: return ImGuiKey.Space;
            case SDL.Keycode.Return: return ImGuiKey.Enter;
            case SDL.Keycode.Escape: return ImGuiKey.Escape;
            case SDL.Keycode.Apostrophe: return ImGuiKey.Apostrophe;
            case SDL.Keycode.Comma: return ImGuiKey.Comma;
            case SDL.Keycode.Minus: return ImGuiKey.Minus;
            case SDL.Keycode.Period: return ImGuiKey.Period;
            case SDL.Keycode.Slash: return ImGuiKey.Slash;
            case SDL.Keycode.Semicolon: return ImGuiKey.Semicolon;
            case SDL.Keycode.Equals: return ImGuiKey.Equal;
            case SDL.Keycode.LeftBracket: return ImGuiKey.LeftBracket;
            case SDL.Keycode.Backslash: return ImGuiKey.Backslash;
            case SDL.Keycode.RightBracket: return ImGuiKey.RightBracket;
            case SDL.Keycode.Grave: return ImGuiKey.GraveAccent;
            case SDL.Keycode.Capslock: return ImGuiKey.CapsLock;
            case SDL.Keycode.ScrollLock: return ImGuiKey.ScrollLock;
            case SDL.Keycode.NumLockClear: return ImGuiKey.NumLock;
            case SDL.Keycode.PrintScreen: return ImGuiKey.PrintScreen;
            case SDL.Keycode.Pause: return ImGuiKey.Pause;
            case SDL.Keycode.LCtrl: return ImGuiKey.LeftCtrl;
            case SDL.Keycode.LShift: return ImGuiKey.LeftShift;
            case SDL.Keycode.LAlt: return ImGuiKey.LeftAlt;
            case SDL.Keycode.LGUI: return ImGuiKey.LeftSuper;
            case SDL.Keycode.RCtrl: return ImGuiKey.RightCtrl;
            case SDL.Keycode.RShift: return ImGuiKey.RightShift;
            case SDL.Keycode.RAlt: return ImGuiKey.RightAlt;
            case SDL.Keycode.RGUI: return ImGuiKey.RightSuper;
            case SDL.Keycode.Application: return ImGuiKey.Menu;
            case SDL.Keycode.Alpha0: return ImGuiKey._0;
            case SDL.Keycode.Alpha1: return ImGuiKey._1;
            case SDL.Keycode.Alpha2: return ImGuiKey._2;
            case SDL.Keycode.Alpha3: return ImGuiKey._3;
            case SDL.Keycode.Alpha4: return ImGuiKey._4;
            case SDL.Keycode.Alpha5: return ImGuiKey._5;
            case SDL.Keycode.Alpha6: return ImGuiKey._6;
            case SDL.Keycode.Alpha7: return ImGuiKey._7;
            case SDL.Keycode.Alpha8: return ImGuiKey._8;
            case SDL.Keycode.Alpha9: return ImGuiKey._9;
            case SDL.Keycode.A: return ImGuiKey.A;
            case SDL.Keycode.B: return ImGuiKey.B;
            case SDL.Keycode.C: return ImGuiKey.C;
            case SDL.Keycode.D: return ImGuiKey.D;
            case SDL.Keycode.E: return ImGuiKey.E;
            case SDL.Keycode.F: return ImGuiKey.F;
            case SDL.Keycode.G: return ImGuiKey.G;
            case SDL.Keycode.H: return ImGuiKey.H;
            case SDL.Keycode.I: return ImGuiKey.I;
            case SDL.Keycode.J: return ImGuiKey.J;
            case SDL.Keycode.K: return ImGuiKey.K;
            case SDL.Keycode.L: return ImGuiKey.L;
            case SDL.Keycode.M: return ImGuiKey.M;
            case SDL.Keycode.N: return ImGuiKey.N;
            case SDL.Keycode.O: return ImGuiKey.O;
            case SDL.Keycode.P: return ImGuiKey.P;
            case SDL.Keycode.Q: return ImGuiKey.Q;
            case SDL.Keycode.R: return ImGuiKey.R;
            case SDL.Keycode.S: return ImGuiKey.S;
            case SDL.Keycode.T: return ImGuiKey.T;
            case SDL.Keycode.U: return ImGuiKey.U;
            case SDL.Keycode.V: return ImGuiKey.V;
            case SDL.Keycode.W: return ImGuiKey.W;
            case SDL.Keycode.X: return ImGuiKey.X;
            case SDL.Keycode.Y: return ImGuiKey.Y;
            case SDL.Keycode.Z: return ImGuiKey.Z;
            case SDL.Keycode.F1: return ImGuiKey.F1;
            case SDL.Keycode.F2: return ImGuiKey.F2;
            case SDL.Keycode.F3: return ImGuiKey.F3;
            case SDL.Keycode.F4: return ImGuiKey.F4;
            case SDL.Keycode.F5: return ImGuiKey.F5;
            case SDL.Keycode.F6: return ImGuiKey.F6;
            case SDL.Keycode.F7: return ImGuiKey.F7;
            case SDL.Keycode.F8: return ImGuiKey.F8;
            case SDL.Keycode.F9: return ImGuiKey.F9;
            case SDL.Keycode.F10: return ImGuiKey.F10;
            case SDL.Keycode.F11: return ImGuiKey.F11;
            case SDL.Keycode.F12: return ImGuiKey.F12;
            case SDL.Keycode.F13: return ImGuiKey.F13;
            case SDL.Keycode.F14: return ImGuiKey.F14;
            case SDL.Keycode.F15: return ImGuiKey.F15;
            case SDL.Keycode.F16: return ImGuiKey.F16;
            case SDL.Keycode.F17: return ImGuiKey.F17;
            case SDL.Keycode.F18: return ImGuiKey.F18;
            case SDL.Keycode.F19: return ImGuiKey.F19;
            case SDL.Keycode.F20: return ImGuiKey.F20;
            case SDL.Keycode.F21: return ImGuiKey.F21;
            case SDL.Keycode.F22: return ImGuiKey.F22;
            case SDL.Keycode.F23: return ImGuiKey.F23;
            case SDL.Keycode.F24: return ImGuiKey.F24;
            case SDL.Keycode.AcBack: return ImGuiKey.AppBack;
            case SDL.Keycode.AcForward: return ImGuiKey.AppForward;
            default: break;
        }

        return ImGuiKey.None;
    }

    public static nint Data() => ImGui.GetIO().BackendPlatformUserData;

    public static void UpdateKeyModifiers(SDL.Keymod keymods)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.AddKeyEvent(ImGuiKey.ModCtrl, (keymods & (SDL.Keymod.LCtrl | SDL.Keymod.RCtrl)) != 0);
        io.AddKeyEvent(ImGuiKey.ModShift, (keymods & (SDL.Keymod.LShift | SDL.Keymod.RShift)) != 0);
        io.AddKeyEvent(ImGuiKey.ModAlt, (keymods & (SDL.Keymod.LAlt | SDL.Keymod.RAlt)) != 0);
        io.AddKeyEvent(ImGuiKey.ModSuper, (keymods & (SDL.Keymod.LGUI | SDL.Keymod.RGUI)) != 0);
    }

    private static void SetupPlatformHandles(ImGuiViewportPtr viewport, nint window)
    {
        viewport.PlatformHandle = window;
        viewport.PlatformHandleRaw = 0;
#if _WIN32 && !__WINTR__
        SDL.GetPointerProperty(SDL.GetWindowProperties(window), SDL.Props.WindowWin32HWNDPointer, 0);
#elif __APPLE__ && SDL_VIDEO_DRIVER_COCOA
        SDL.GetPointerProperty(SDL.GetWindowProperties(window), SDL.Props.WindowCocoaWindow, 0);
#endif
    }
}