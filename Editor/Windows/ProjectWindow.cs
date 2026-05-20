using System.Reflection;
using Editor.Core.Types;
using Engine.Core;
using ImGuiNET;
using OpenTK.Windowing.Common;

namespace Editor.Windows;

public sealed class ProjectWindow : IEditorWindow, IDisposable
{
    public string Title => "Project";
    public EditorWindowSlot Slot => EditorWindowSlot.BottomFullWidth;

    private List<Type> prefabs = [];

    public ProjectWindow()
    {
        foreach (var gameObject in Assembly.GetExecutingAssembly().GetTypes()
                     .ToList())
        {
            Logger.LogWarning($"Loaded prefab: {gameObject.Name}");
            prefabs.Add(gameObject);
        }
    }

    public void Render(FrameEventArgs e)
    {
        ImGui.BeginChild("##editor-project-prefabs", new System.Numerics.Vector2(0, -ImGui.GetFrameHeightWithSpacing()));
        foreach (var prefab in prefabs)
        {
            ImGui.Button(prefab.Name);
        }
        ImGui.EndChild();
    }

    public void Update(FrameEventArgs e)
    {
    }

    public void Dispose()
    {
    }
}