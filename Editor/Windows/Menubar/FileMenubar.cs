using Editor.Core.Types;
using Engine.Core;
using ImGuiNET;
using System.Windows.Forms;
using Engine;
using Engine.Rendering;
using Metadata = Editor.Core.Metadata;

namespace Editor.Windows.Menubar;

public static class FileMenubar
{
    public static void Render()
    {
        if (!ImGui.BeginMenu($"{Metadata.EDITOR_NAME} {Metadata.EDITOR_VERSION}")) return;

        if (ImGui.MenuItem("New Map", "Ctrl+N"))
        {
            LatibuleEngine.Map = new GameMap();
        }

        if (ImGui.MenuItem("Open", "Ctrl+O"))
        {
            // Open map
        }

        if (ImGui.MenuItem("Save", "Ctrl+S"))
        {
            // Save map
        }

        if (ImGui.MenuItem("Save As..."))
        {
            // Save map as
        }

        ImGui.Separator();

        if (ImGui.MenuItem("Exit")) EngineStates.GameWindow.Close();

        ImGui.EndMenu();
    }
}