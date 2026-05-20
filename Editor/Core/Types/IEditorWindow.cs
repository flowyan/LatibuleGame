using Editor.Windows;
using OpenTK.Windowing.Common;

namespace Editor.Core.Types;

public interface IEditorWindow
{
    string? Title { get; }
    EditorWindowSlot Slot { get; }
    void Render(FrameEventArgs e);
    void Update(FrameEventArgs e);
}

