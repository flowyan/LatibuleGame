using Engine.Core;
using SDL3;

namespace Editor.Core;

public class EditorKeybinds
{
    public static void Initialize()
    {
        Input.BindKeyPressed(SDL.Scancode.B, () => EditorOptions.RenderBoundingBoxes = !EditorOptions.RenderBoundingBoxes);
        Input.BindKeyPressed(SDL.Scancode.P, () => EditorOptions.UpdateMap = !EditorOptions.UpdateMap);
    }
}