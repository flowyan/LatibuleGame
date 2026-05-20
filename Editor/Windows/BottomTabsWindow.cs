using Editor.Core.Types;
using ImGuiNET;
using OpenTK.Windowing.Common;

namespace Editor.Windows;

public sealed class BottomTabsWindow : IEditorWindow, IDisposable
{
    public string? Title => null;
    public EditorWindowSlot Slot => EditorWindowSlot.BottomFullWidth;
    
    private readonly ProjectWindow projectWindow = new();
    private readonly ConsoleWindow consoleWindow = new();

    public void Render(FrameEventArgs e)
    {
        ImGui.BeginTabBar("Tabs");
        if (ImGui.BeginTabItem("Project"))
        {
            projectWindow.Render(e);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Console"))
        {
            consoleWindow.Render(e);
            ImGui.EndTabItem();
        }
            
        
        ImGui.EndTabBar();
    }

    public void Update(FrameEventArgs e)
    {
        projectWindow.Update(e);
        consoleWindow.Update(e);
    }

    public void Dispose()
    {
        projectWindow.Dispose();
        consoleWindow.Dispose();
    }
}