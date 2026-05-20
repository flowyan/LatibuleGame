using Assimp;
using JoltPhysicsSharp;
using OpenTK.Mathematics;

namespace Engine.Utilities;

public static class PhysSettings
{
    public static StaticCompoundShapeSettings StaticCompoundShape(Scene model, int[]? onlyMeshes = null)
    {
        var compoundSettings = new StaticCompoundShapeSettings();
        
        for (var index = 0; index < model.Meshes.Count; index++)
        {
            if (onlyMeshes != null) if (onlyMeshes.Contains(index)) continue;
            var mesh = model.Meshes[index];
            var vertices = new List<System.Numerics.Vector3>();
            foreach (var vertex in mesh.Vertices)
            {
                vertices.Add(new System.Numerics.Vector3(vertex.X, vertex.Y, vertex.Z));
            }

            using var convexHullSettings = new ConvexHullShapeSettings(vertices.ToArray());
            var shapeResult = convexHullSettings.Create();

            compoundSettings.AddShape(Vector3.Zero.ToNumerics(), System.Numerics.Quaternion.Identity, shapeResult);
        }

        return compoundSettings;
    }
}