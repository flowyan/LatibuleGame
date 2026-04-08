using Editor.Core.Types;
using ImGuiNET;
using OpenTK.Windowing.Common;

namespace Editor.Windows;

public sealed class InspectorWindow : IEditorWindow
{
    public string Title => "Inspector";
    public EditorWindowSlot Slot => EditorWindowSlot.TopRight;

    public void Render(FrameEventArgs e)
    {
        ImGui.Text("Selection details");
        ImGui.Separator();
        ImGui.TextDisabled("Select an object to inspect its components.");
    }
}

