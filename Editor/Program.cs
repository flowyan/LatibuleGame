using Engine.Windowing;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SDL3;
using Metadata = Editor.Core.Metadata;

namespace Editor;

public static class Program
{
    private static readonly NativeWindowSettings _windowSettings = new()
    {
        ClientSize = new Vector2i(1920, 1080),
        Title = $"{Metadata.EDITOR_NAME} {Metadata.EDITOR_VERSION}",
        // Icon = WindowHelper.LoadIcon("Assets/icon.jpg"),
        APIVersion = new Version(4, 6),
        Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,
        Profile = ContextProfile.Core,
        Vsync = VSyncMode.Off,
        NumberOfSamples = 4,
    };
    
    private static readonly SDLNativeWindowSettings _settings = new()
    {
        ClientSize = new Vector2i(1920, 1080),
        Title = $"{Metadata.EDITOR_NAME} {Metadata.EDITOR_VERSION}",
        IconPath = "Assets/icon.jpg",
        APIVersion = new Version(4, 6),
    };

    public static void Main(string[] args)
    {
        EngineAppWindow.CreateGame = () => new MalletEditor(_settings);
        SDL.RunMainCallbacks<EngineAppWindow>(args);
    }
}