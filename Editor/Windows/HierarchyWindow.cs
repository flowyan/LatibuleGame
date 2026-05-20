using Editor.Core.Types;
using Engine;
using ImGuiNET;
using OpenTK.Windowing.Common;

namespace Editor.Windows;

public sealed class HierarchyWindow : IEditorWindow
{
    public string Title => "Hierarchy";
    public EditorWindowSlot Slot => EditorWindowSlot.TopLeft;

    public unsafe void Render(FrameEventArgs e)
    {
        ImGui.Text("Scene graph");
        ImGui.Separator();

        var counter = 0;
        foreach (var go in LatibuleEngine.Map.Objects)
        {
            counter++;
            if (go.Parent != null) continue;
            if (ImGui.SmallButton($"[{counter.ToString().PadLeft(3, '0')}] {go.EditorName()}")) MalletEditor.SelectObject(go);
            if (go.Children.Any())
            {
                ImGui.Indent();
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1f));
                foreach (var child in go.Children)
                {
                    if (ImGui.SmallButton($"└ {child.EditorName()}")) MalletEditor.SelectObject(go);
                }

                ImGui.PopStyleColor();

                ImGui.Unindent();
            }
        }
    }

    public void Update(FrameEventArgs e)
    {
    }
}