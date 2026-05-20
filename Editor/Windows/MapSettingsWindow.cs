using System.Reflection;
using Editor.Core;
using Editor.Core.Types;
using Engine;
using Engine.Core;
using Engine.Core.Types;
using ImGuiNET;
using JoltPhysicsSharp;
using OpenTK.Windowing.Common;

namespace Editor.Windows;

public sealed class MapSettingsWindow : IEditorWindow, IDisposable
{
    public string Title => "Map Settings";
    public EditorWindowSlot Slot => EditorWindowSlot.BottomRight;

    public void Render(FrameEventArgs e)
    {
        ImGui.Checkbox("Render Bounding Boxes", ref EditorOptions.RenderBoundingBoxes);
        ImGui.Checkbox("Update Map", ref EditorOptions.UpdateMap);
        if (ImGui.Button("Delete Orphaned Bodies"))
        {
            var usedBodyIds = LatibuleEngine.Map.Objects
                .Where(go => go.PhysicsBodyID.HasValue)
                .Select(go => go.PhysicsBodyID)
                .ToHashSet();

            var orphanedBodyIds = LatibuleEngine.Physics.Bodies
                .Select(body => body.ID)
                .Where(bodyId => !usedBodyIds.Contains(bodyId))
                .ToList();

            foreach (var bodyId in orphanedBodyIds)
            {
                LatibuleEngine.Physics.BodyInterface.RemoveAndDestroyBody(bodyId);
            }

            LatibuleEngine.Physics.Bodies.RemoveAll(body => orphanedBodyIds.Contains(body.ID));

            Logger.LogInfo($"Destroyed {orphanedBodyIds.Count} orphaned physics bodies.");
        }
    }

    public void Update(FrameEventArgs e)
    {
    }

    public void Dispose()
    {
    }
}