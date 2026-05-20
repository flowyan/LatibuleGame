using Editor.Core.Types;
using Engine.Core;
using ImGuiNET;
using Engine;
using Engine.Rendering;
using Engine.Serialization;
using Latibule.Maps;
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
            var loadedMap = MapFileUtil.LoadMapFromFile();
            if (loadedMap != null) LatibuleEngine.Map = loadedMap;
            LatibuleEngine.Map.OnLoad();
            // Open map
        }

        if (ImGui.MenuItem("evil", "penis"))
        {
            LatibuleEngine.Map = new GameMap();
            LatibuleEngine.Map = TestingMap.Create();
            LatibuleEngine.Map.OnLoad();
        }

        if (ImGui.MenuItem("Save", "Ctrl+S"))
        {
            MapFileUtil.SaveMapToFile();
            // Save map
        }

        ImGui.Separator();
        if (ImGui.MenuItem("About"))
        {
        }

        ImGui.Separator();

        if (ImGui.MenuItem("Exit")) EngineStates.GameWindow.Close();

        ImGui.EndMenu();
    }
}