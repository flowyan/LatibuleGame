using OpenTK.Mathematics;

namespace Engine.Windowing;

public class SDLNativeWindowSettings
{
    public static readonly SDLNativeWindowSettings Default = new SDLNativeWindowSettings();

    public SDLNativeWindowSettings()
    {
        unsafe
        {
        }
    }

    /// <summary>
    /// Gets or sets the title of the new window.
    /// </summary>
    public string Title { get; set; } = "OpenTK Window";

    /// <summary>Path to the window icon. Null means no custom icon.</summary>
    public string? IconPath { get; set; }

    public Vector2i ClientSize { get; set; } = new(1280, 720);

    /// <summary>
    /// Gets or sets a value representing the current version of the graphics API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// OpenGL 3.3 is selected by default, and runs on almost any hardware made within the last ten years.
    /// This will run on Windows, Mac OS, and Linux.
    /// </para>
    /// <para>
    /// OpenGL 4.1 is suggested for modern apps meant to run on more modern hardware.
    /// This will run on Windows, Mac OS, and Linux.
    /// </para>
    /// <para>
    /// OpenGL 4.6 is suggested for modern apps that only intend to run on Windows and Linux;
    /// Mac OS doesn't support it.
    /// </para>
    /// <para>
    /// Note that if you choose an API other than base OpenGL, this will need to be updated accordingly,
    /// as the versioning of OpenGL and OpenGL ES do not match.
    /// </para>
    /// </remarks>
    public Version APIVersion { get; set; } = new Version(3, 3);
}