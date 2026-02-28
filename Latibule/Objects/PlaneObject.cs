using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Shapes;
using OpenTK.Mathematics;

namespace Latibule.Objects;

/// <summary>
/// A flat plane for ground/floor rendering and collision.
/// </summary>
public class PlaneObject : GameObject
{
    public PlaneObject()
    {
        // Default plane transform
        Transform = new Transform(Vector3.Zero, new Vector3(1, 0, 1), Vector3.Zero);
    }

    public override void OnLoad()
    {
        base.OnLoad();

        WithComponents([
            new ShaderComponent(Asseteer.GetShader(ShaderAsset.mesh_shader)),
            new ShapeRendererComponent(new PlaneShape()),
            new BoundingBoxComponent()
        ]);
    }
}