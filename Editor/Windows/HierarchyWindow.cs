using Editor.Core.Types;
using Engine;
using ImGuiNET;
using OpenTK.Windowing.Common;

namespace Editor.Windows;

public sealed class HierarchyWindow : IEditorWindow
{
    public string Title => "Hierarchy";
    public EditorWindowSlot Slot => EditorWindowSlot.TopLeft;

    public void Render(FrameEventArgs e)
    {
        ImGui.Text("Scene graph");
        ImGui.Separator();

        foreach (var gameObject in LatibuleEngine.Map.Objects)
        {
            ImGui.Text(gameObject.ToString());
        }
    }
}

