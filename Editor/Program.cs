using Editor.Core;
using Engine.Core;
using Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Metadata = Editor.Core.Metadata;

namespace Editor;

public static class Program
{
    private static readonly NativeWindowSettings _windowSettings = new()
    {
        ClientSize = new Vector2i(1920, 1080),
        Title = $"{Metadata.EDITOR_NAME} {Metadata.EDITOR_VERSION}",
        Icon = WindowHelper.LoadIcon("Assets/icon.jpg"),
        APIVersion = new Version(4, 6),
        Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,
        Profile = ContextProfile.Core,
        Vsync = VSyncMode.Off,
        NumberOfSamples = 4,
    };

    public static void Main()
    {
        using var editor = new MalletEditor(_windowSettings);
        editor.Run();
    }
}