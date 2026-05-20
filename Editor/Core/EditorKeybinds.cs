using Engine.Core;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Editor.Core;

public class EditorKeybinds
{
    public static void Initialize()
    {
        Input.BindKeyPressed(Keys.B, () => EditorOptions.RenderBoundingBoxes = !EditorOptions.RenderBoundingBoxes);
        Input.BindKeyPressed(Keys.P, () => EditorOptions.UpdateMap = !EditorOptions.UpdateMap);
    }
}