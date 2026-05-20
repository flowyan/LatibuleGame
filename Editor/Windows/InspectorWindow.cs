using Editor.Core.Types;
using Engine;
using Engine.Core;
using Engine.Utilities;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Editor.Windows;

public sealed class InspectorWindow : IEditorWindow
{
    public string Title => "Inspector";
    public EditorWindowSlot Slot => EditorWindowSlot.TopRight;

    public void Render(FrameEventArgs e)
    {
        ImGui.Separator();
        if (MalletEditor.SelectedObjects == null)
        {
            ImGui.TextDisabled("Select an object to inspect its components.");
            return;
        }

        if (MalletEditor.SelectedObjects.Count() > 1)
        {
            ImGui.TextDisabled("Multiple objects selected. Select a single object to inspect its components.");
            return;
        }
        var obj =  MalletEditor.SelectedObjects.First();
        var pos = obj.Transform.Position;
        var rot = obj.Transform.Rotation;
        var scale = obj.Transform.Scale;
        
        ImGui.Text(obj.ToString());
        ImGui.Separator();
        ImGui.Text("Position");
        ImGui.InputFloat("X: ##posX", ref pos.X, 0.1f);
        ImGui.InputFloat("Y: ##posY", ref pos.Y, 0.1f);
        ImGui.InputFloat("Z: ##posZ", ref pos.Z, 0.1f);
        if (pos != obj.Transform.Position) obj.Transform.Position = pos;
        ImGui.Separator();
        ImGui.Text("Rotation");
        ImGui.InputFloat("X: ##rotX", ref rot.X, Step);
        ImGui.InputFloat("Y: ##rotY", ref rot.Y, Step);
        ImGui.InputFloat("Z: ##rotZ", ref rot.Z, Step);
        if (rot != obj.Transform.Rotation) obj.Transform.Rotation = rot;
        
        ImGui.Separator();
        ImGui.Text("Scale");
        ImGui.InputFloat("X: ##scaleX", ref scale.X, 0.1f);
        ImGui.InputFloat("Y: ##scaleY", ref scale.Y, 0.1f);
        ImGui.InputFloat("Z: ##scaleZ", ref scale.Z, 0.1f);
        if (scale != obj.Transform.Scale) obj.Transform.Scale = scale;
        ImGui.Separator();
        ImGui.Text("Step");
        ImGui.InputFloat("##rotStep", ref Step, 5f);
        
        
        if (ImGui.Button("Go To"))
        {
            LatibuleEngine.Camera.Position = obj.Transform.Position;
            // LatibuleEngine.Camera.Position = obj.Transform.Position + new Vector3(0, 0, 5);
            // LatibuleEngine.Camera.Direction = Vector3.Normalize(obj.Transform.Position - LatibuleEngine.Camera.Position);
            LatibuleEngine.Camera.Update();
        }
        ImGui.SameLine();
        if (ImGui.Button("OnLoad"))
        {
            obj.OnLoad();
        }
    }

    public void Update(FrameEventArgs e)
    {
        var deltaTime = (float)e.Time;
        
        if (MalletEditor.SelectedObjects != null)
        {
            var move = Vector3.Zero;
            var speed = 5f;
            var forward = Vector3.Normalize(new Vector3(LatibuleEngine.Camera.Direction.X, 0, LatibuleEngine.Camera.Direction.Z));
            var right = Vector3.Normalize(Vector3.Cross(forward, Vector3Direction.Up));

            if (Input.IsKeyDown(Keys.Up)) move += forward;
            if (Input.IsKeyDown(Keys.Down)) move -= forward;
            if (Input.IsKeyDown(Keys.Left)) move -= right;
            if (Input.IsKeyDown(Keys.Right)) move += right;
            if (Input.IsKeyDown(Keys.PageUp)) move += Vector3Direction.Up;
            if (Input.IsKeyDown(Keys.PageDown)) move += Vector3Direction.Down;
            if (move != Vector3.Zero) move = Vector3.Normalize(move);

            foreach (var go in MalletEditor.SelectedObjects)
            {
                go.Transform.Position += move *  speed * deltaTime;
                go.SyncPhysicsBodyID();
            }
        }
    }

    private static float Step = 5f;
}

